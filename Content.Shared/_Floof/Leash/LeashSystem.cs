using System.Linq;
using Content.Shared.Clothing.Components;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Floofstation.Leash.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Input;
using Content.Shared.Interaction;
using Content.Shared.Inventory.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Robust.Shared.Input.Binding;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Dynamics.Joints;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;


namespace Content.Shared.Floofstation.Leash;

public sealed class LeashSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfters = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedJointSystem _joints = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;

    public static VerbCategory LeashLengthConfigurationCategory =
        new("verb-categories-leash-config", "/Textures/_Floof/Interface/VerbIcons/resize.svg.192dpi.png");

    #region Lifecycle

    public override void Initialize()
    {
        UpdatesBefore.Add(typeof(SharedPhysicsSystem));

        SubscribeLocalEvent<LeashAnchorComponent, BeingUnequippedAttemptEvent>(OnAnchorUnequipping);
        SubscribeLocalEvent<LeashAnchorComponent, GetVerbsEvent<EquipmentVerb>>(OnGetEquipmentVerbs);
        SubscribeLocalEvent<LeashedComponent, JointRemovedEvent>(OnJointRemoved, after: [typeof(SharedJointSystem)]);
        SubscribeLocalEvent<LeashedComponent, GetVerbsEvent<InteractionVerb>>(OnGetLeashedVerbs);

        SubscribeLocalEvent<LeashComponent, ExaminedEvent>(OnLeashExamined);
        SubscribeLocalEvent<LeashComponent, EntGotInsertedIntoContainerMessage>(OnLeashInserted);
        SubscribeLocalEvent<LeashComponent, EntGotRemovedFromContainerMessage>(OnLeashRemoved);
        SubscribeLocalEvent<LeashComponent, GetVerbsEvent<AlternativeVerb>>(OnGetLeashVerbs);

        SubscribeLocalEvent<LeashAnchorComponent, LeashAttachDoAfterEvent>(OnAttachDoAfter);
        SubscribeLocalEvent<LeashedComponent, LeashDetachDoAfterEvent>(OnDetachDoAfter);

        CommandBinds.Builder
            .BindBefore(ContentKeyFunctions.MovePulledObject, new PointerInputCmdHandler(OnRequestPullLeash), before: [typeof(PullingSystem)])
            .Register<LeashSystem>();
    }

    public override void Shutdown()
    {
        base.Shutdown();
        CommandBinds.Unregister<LeashSystem>();
    }

    public override void Update(float frameTime)
    {
        var leashQuery = EntityQueryEnumerator<LeashComponent, PhysicsComponent>();

        while (leashQuery.MoveNext(out var leashEnt, out var leash, out var physics))
        {
            var sourceXForm = Transform(leashEnt);
            foreach (var data in leash.Leashed.ToList())
                UpdateLeash(data, sourceXForm, leash, leashEnt);

            // Server - ensure the holder of the leash is always correct
            // I do not know why, perhaps because RobustToolbox tooling is shitty,
            // but if the leash is inside a container that is inside another container (e.g. person inside a locker),
            // and then the middle container leaves the outer (person leaves the locker),
            // RobustToolbox won't update the joint between the leashed person and the leash (which should be relayed to the outer container - locker).
            // This means the person will stay attached to the outer container (locker).
            // To fix this, we do this expensive (but mandatory) computation and recreate the joint when this occurs.
            // Luckily for us, this only happens with the leash, not with the leashed person, thanks to the way we handle anchors.
            if (_net.IsServer
                && TryComp<JointComponent>(leashEnt, out var leashJointComp)
                && _container.TryGetOuterContainer(leashEnt, sourceXForm, out var jointRelayTarget)
                && leashJointComp.Relay != null
                && leashJointComp.Relay != jointRelayTarget.Owner
            )
                _joints.RefreshRelay(leashEnt);
        }

        leashQuery.Dispose();
    }

    private void UpdateLeash(LeashComponent.LeashData data, TransformComponent sourceXForm, LeashComponent leash, EntityUid leashEnt)
    {
        if (data.Pulled == NetEntity.Invalid || !TryGetEntity(data.Pulled, out var target))
            return;

        DistanceJoint? joint = null;
        if (data.JointId is not null
            && TryComp<JointComponent>(target, out var jointComp)
            && jointComp.GetJoints.TryGetValue(data.JointId, out var _joint)
        )
            joint = _joint as DistanceJoint;

        // Client: set max distance to infinity to prevent the client from ever predicting leashes.
        if (_net.IsClient)
        {
            if (joint is not null)
                joint.MaxLength = float.MaxValue;

            return;
        }

        // Server: break each leash joint whose entities are on different maps or are too far apart
        var targetXForm = Transform(target.Value);
        if (targetXForm.MapUid != sourceXForm.MapUid
            || !sourceXForm.Coordinates.TryDistance(EntityManager, targetXForm.Coordinates, out var dst)
            || dst > leash.MaxDistance
        )
            RemoveLeash(target.Value, (leashEnt, leash));

        // Server: update leash lengths if necessary/possible
        // The length can be increased freely, but can only be decreased if the pulled entity is close enough
        if (joint is not null && joint.MaxLength > leash.Length && joint.Length < joint.MaxLength)
            joint.MaxLength = Math.Max(joint.Length, leash.Length);

        if (joint is not null && joint.MaxLength < leash.Length)
            joint.MaxLength = leash.Length;
    }

    #endregion

    #region event handling

    private void OnAnchorUnequipping(Entity<LeashAnchorComponent> ent, ref BeingUnequippedAttemptEvent args)
    {
        // Prevent unequipping the anchor clothing until the leash is removed
        if (TryGetLeashTarget(args.Equipment, out var leashTarget)
            && TryComp<LeashedComponent>(leashTarget, out var leashed)
            && leashed.Puller is not null
            && leashed.Anchor == args.Equipment
           )
            args.Cancel();
    }

    private void OnGetEquipmentVerbs(Entity<LeashAnchorComponent> ent, ref GetVerbsEvent<EquipmentVerb> args)
    {
        if (!args.CanInteract
            || !TryGetLeashTarget(ent!, out var leashTarget)
            || !_interaction.InRangeUnobstructed(args.User, leashTarget) // Can't use CanAccess here since clothing
            || args.Using is not { } leash
            || !TryComp<LeashComponent>(leash, out var leashComp))
            return;

        var user = args.User;
        var leashVerb = new EquipmentVerb { Text = Loc.GetString("verb-leash-text") };

        if (CanLeash(ent, (leash, leashComp)))
            leashVerb.Act = () => TryLeash(ent, (leash, leashComp), user);
        else
        {
            leashVerb.Message = Loc.GetString("verb-leash-error-message");
            leashVerb.Disabled = true;
        }

        args.Verbs.Add(leashVerb);


        if (!TryComp<LeashedComponent>(leashTarget, out var leashedComp)
            || leashedComp.Puller != leash
            || HasComp<LeashedComponent>(ent)) // This one means that OnGetLeashedVerbs will add a verb to remove it
            return;

        var unleashVerb = new EquipmentVerb
        {
            Text = Loc.GetString("verb-unleash-text"),
            Act = () => TryUnleash((leashTarget, leashedComp), (leash, leashComp), user)
        };
        args.Verbs.Add(unleashVerb);
    }

    private void OnGetLeashedVerbs(Entity<LeashedComponent> ent, ref GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess
            || !args.CanInteract
            || ent.Comp.Puller is not { } leash
            || !TryComp<LeashComponent>(leash, out var leashComp))
            return;

        var user = args.User;
        args.Verbs.Add(new InteractionVerb
        {
            Text = Loc.GetString("verb-unleash-text"),
            Act = () => TryUnleash(ent!, (leash, leashComp), user)
        });
    }

    private void OnGetLeashVerbs(Entity<LeashComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || ent.Comp.LengthConfigs is not { } configurations)
            return;

        // Add a menu listing each length configuration
        foreach (var length in configurations)
        {
            args.Verbs.Add(new AlternativeVerb
            {
                Text = Loc.GetString("verb-leash-set-length-text", ("length", length)),
                Act = () => SetLeashLength(ent, length),
                Category = LeashLengthConfigurationCategory
            });
        }
    }

    private void OnJointRemoved(Entity<LeashedComponent> ent, ref JointRemovedEvent args)
    {
        var id = args.Joint.ID;
        if (_timing.ApplyingState
            || ent.Comp.LifeStage >= ComponentLifeStage.Removing
            || ent.Comp.Puller is not { } puller
            || !TryComp<LeashAnchorComponent>(ent.Comp.Anchor, out var anchor)
            || !TryComp<LeashComponent>(puller, out var leash)
            || leash.Leashed.All(it => it.JointId != id))
            return;

        RemoveLeash(ent!, (puller, leash), false);

        // If the entity still has a leashed comp, and is on the same map, and is within the max distance of the leash
        // Then the leash was likely broken due to some weird unforeseen fucking robust toolbox magic.
        // We can try to recreate it, but on the next tick.
        Timer.Spawn(0, () =>
        {
            if (TerminatingOrDeleted(ent.Comp.Anchor.Value)
                || TerminatingOrDeleted(puller)
                || !Transform(ent).Coordinates.TryDistance(EntityManager, Transform(puller).Coordinates, out var dst)
                || dst > leash.MaxDistance
            )
                return;

            DoLeash((ent.Comp.Anchor.Value, anchor), (puller, leash), ent);
        });
    }

    private void OnLeashExamined(Entity<LeashComponent> ent, ref ExaminedEvent args)
    {
        var length = ent.Comp.Length;
        args.PushMarkup(Loc.GetString("leash-length-examine-text", ("length", length)));
    }

    private void OnLeashInserted(Entity<LeashComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        if (!_net.IsClient)
            RefreshJoints(ent);
    }

    private void OnLeashRemoved(Entity<LeashComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        if (!_net.IsClient)
            RefreshJoints(ent);
    }

    private void OnAttachDoAfter(Entity<LeashAnchorComponent> ent, ref LeashAttachDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled
            || !TryComp<LeashComponent>(args.Used, out var leash)
            || !CanLeash(ent, (args.Used.Value, leash)))
            return;

        DoLeash(ent, (args.Used.Value, leash), EntityUid.Invalid);
    }

    private void OnDetachDoAfter(Entity<LeashedComponent> ent, ref LeashDetachDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || ent.Comp.Puller is not { } leash)
            return;

        RemoveLeash(ent!, leash);
    }

    private bool OnRequestPullLeash(ICommonSession? session, EntityCoordinates targetCoords, EntityUid uid)
    {
        if (_net.IsClient
            || session?.AttachedEntity is not { } player
            || !player.IsValid()
            || !TryComp<HandsComponent>(player, out var hands)
            || hands.ActiveHandEntity is not {} leash
            || !TryComp<LeashComponent>(leash, out var leashComp)
            || leashComp.NextPull > _timing.CurTime)
            return false;

        // find the entity closest to the target coords
        var candidates = leashComp.Leashed
            .Select(it => GetEntity(it.Pulled))
            .Where(it => it != EntityUid.Invalid)
            .Select(it => (it, Transform(it).Coordinates.TryDistance(EntityManager, _xform, targetCoords, out var dist) ? dist : float.PositiveInfinity))
            .Where(it => it.Item2 < float.PositiveInfinity)
            .ToList();

        if (candidates.Count == 0)
            return false;

        // And pull it towards the user
        var pulled = candidates.MinBy(it => it.Item2).Item1;
        var playerCoords = Transform(player).Coordinates;
        var pulledCoords = Transform(pulled).Coordinates;
        var pullDir = _xform.ToMapCoordinates(playerCoords).Position - _xform.ToMapCoordinates(pulledCoords).Position;

        _throwing.TryThrow(pulled, pullDir * 0.5f, user: player, pushbackRatio: 1f, animated: false, recoil: false, playSound: false, doSpin: false);

        leashComp.NextPull = _timing.CurTime + leashComp.PullInterval;
        return true;
    }

    #endregion

    #region private api

    /// <summary>
    ///     Tries to find the entity that gets leashed for the given anchor entity.
    /// </summary>
    private bool TryGetLeashTarget(Entity<LeashAnchorComponent?> ent, out EntityUid leashTarget)
    {
        leashTarget = default;
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        if (ent.Comp.Kind.HasFlag(LeashAnchorComponent.AnchorKind.Clothing)
            && TryComp<ClothingComponent>(ent, out var clothing)
            && clothing.InSlot != null
            && _container.TryGetContainingContainer(ent.Owner, out var container)) // DeltaV - use owner
        {
            leashTarget = container.Owner;
            return true;
        }

        if (ent.Comp.Kind.HasFlag(LeashAnchorComponent.AnchorKind.Intrinsic))
        {
            leashTarget = ent.Owner;
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Returns true if a leash joint can be created between the two specified entities.
    ///     This will return false if one of the entities is a parent of another.
    /// </summary>
    public bool CanCreateJoint(EntityUid a, EntityUid b)
    {
        BaseContainer? aOuter = null, bOuter = null;

        // If neither of the entities are in contianers, it's safe to create a joint
        if (!_container.TryGetOuterContainer(a, Transform(a), out aOuter)
            && !_container.TryGetOuterContainer(b, Transform(b), out bOuter))
            return true;

        // Otherwise, we need to make sure that neither of the entities contain the other, and that they are not in the same container.
        return a != bOuter?.Owner && b != aOuter?.Owner && aOuter?.Owner != bOuter?.Owner;
    }

    private DistanceJoint CreateLeashJoint(string jointId, Entity<LeashComponent> leash, EntityUid leashTarget)
    {
        var joint = _joints.CreateDistanceJoint(leash, leashTarget, id: jointId);
        // If the soon-to-be-leashed entity is too far away, we don't force it any closer.
        // The system will automatically reduce the length of the leash once it gets closer.
        var length = Transform(leashTarget).Coordinates.TryDistance(EntityManager, Transform(leash).Coordinates, out var dist)
            ? MathF.Max(dist, leash.Comp.Length)
            : leash.Comp.Length;

        joint.MinLength = 0f;
        joint.MaxLength = length;
        joint.Stiffness = 1f;
        joint.CollideConnected = true; // This is just for performance reasons and doesn't actually make mobs collide.
        joint.Damping = 1f;

        return joint;
    }

    #endregion

    #region public api

    public bool CanLeash(Entity<LeashAnchorComponent> anchor, Entity<LeashComponent> leash)
    {
        return leash.Comp.Leashed.Count < leash.Comp.MaxJoints
            && TryGetLeashTarget(anchor!, out var leashTarget)
            && CompOrNull<LeashedComponent>(leashTarget)?.JointId == null
            && Transform(anchor).Coordinates.TryDistance(EntityManager, Transform(leash).Coordinates, out var dst)
            && dst <= leash.Comp.Length;
    }


    public bool TryLeash(Entity<LeashAnchorComponent> anchor, Entity<LeashComponent> leash, EntityUid user, bool popup = true)
    {
        if (!CanLeash(anchor, leash) || !TryGetLeashTarget(anchor!, out var leashTarget))
            return false;

        var doAfter = new DoAfterArgs(EntityManager, user, leash.Comp.AttachDelay, new LeashAttachDoAfterEvent(), anchor, leashTarget, leash)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnWeightlessMove = false,
            NeedHand = true
        };

        var result = _doAfters.TryStartDoAfter(doAfter);
        if (result && _net.IsServer && popup)
        {
            (string, object)[] locArgs = [("user", user), ("target", leashTarget), ("anchor", anchor.Owner), ("selfAnchor", anchor.Owner == leashTarget)];

            // This could've been much easier if my interaction verbs PR got merged already, but it isn't yet, so I gotta suffer
            _popups.PopupEntity(Loc.GetString("leash-attaching-popup-self", locArgs), user, user);
            if (user != leashTarget)
                _popups.PopupEntity(Loc.GetString("leash-attaching-popup-target", locArgs), leashTarget, leashTarget);

            var othersFilter = Filter.PvsExcept(leashTarget).RemovePlayerByAttachedEntity(user);
            _popups.PopupEntity(Loc.GetString("leash-attaching-popup-others", locArgs), leashTarget, othersFilter, true);
        }
        return result;
    }

    public bool TryUnleash(Entity<LeashedComponent?> leashed, Entity<LeashComponent?> leash, EntityUid user, bool popup = true)
    {
        if (!Resolve(leashed, ref leashed.Comp, false) || !Resolve(leash, ref leash.Comp) || leashed.Comp.Puller != leash)
            return false;

        var delay = user == leashed.Owner ? leash.Comp.SelfDetachDelay : leash.Comp.DetachDelay;
        var doAfter = new DoAfterArgs(EntityManager, user, delay, new LeashDetachDoAfterEvent(), leashed.Owner, leashed)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnWeightlessMove = false,
            NeedHand = true
        };

        var result = _doAfters.TryStartDoAfter(doAfter);
        if (result && _net.IsServer)
        {
            (string, object)[] locArgs = [("user", user), ("target", leashed.Owner), ("isSelf", user == leashed.Owner)];
            _popups.PopupEntity(Loc.GetString("leash-detaching-popup-self", locArgs), user, user);
            _popups.PopupEntity(Loc.GetString("leash-detaching-popup-others", locArgs), user, Filter.PvsExcept(user), true);
        }

        return result;
    }

    /// <summary>
    ///     Immediately creates the leash joint between the specified entities and sets up respective components.
    /// </summary>
    /// <param name="anchor">The anchor entity, usually either target's clothing or the target itself.</param>
    /// <param name="leash">The leash entity.</param>
    /// <param name="leashTarget">The entity to which the leash is actually connected. Can be EntityUid.Invalid, then it will be deduced.</param>
    /// <param name="force">Whether to force the leash to be created even if the target is too far away.</param>
    public void DoLeash(Entity<LeashAnchorComponent> anchor, Entity<LeashComponent> leash, EntityUid leashTarget, bool force = false)
    {
        if (_net.IsClient || leashTarget is { Valid: false } && !TryGetLeashTarget(anchor!, out leashTarget))
            return;

        // Do not allow to create the joint if the target is too far away - this is mostly to prevent re-creating leashes after teleportation
        if (!force &&
            Transform(anchor).Coordinates.TryDistance(EntityManager, Transform(leash).Coordinates, out var dst) &&
            dst > leash.Comp.MaxDistance)
            return;

        var leashedComp = EnsureComp<LeashedComponent>(leashTarget);
        var netLeashTarget = GetNetEntity(leashTarget);
        var data = new LeashComponent.LeashData(null, netLeashTarget);

        leashedComp.Puller = leash;
        leashedComp.Anchor = anchor;

        if (CanCreateJoint(leashTarget, leash))
        {
            var jointId = $"leash-joint-{netLeashTarget}";
            var joint = CreateLeashJoint(jointId, leash, leashTarget);
            data.JointId = leashedComp.JointId = jointId;
        }
        else
        {
            leashedComp.JointId = null;
        }

        if (leash.Comp.LeashSprite is { } sprite)
        {
            _container.EnsureContainer<ContainerSlot>(leashTarget, LeashedComponent.VisualsContainerName);
            if (EntityManager.TrySpawnInContainer(null, leashTarget, LeashedComponent.VisualsContainerName, out var visualEntity))
            {
                var visualComp = EnsureComp<LeashedVisualsComponent>(visualEntity.Value);
                visualComp.Sprite = sprite;
                visualComp.Source = leash;
                visualComp.Target = leashTarget;
                visualComp.OffsetTarget = anchor.Comp.Offset;

                data.LeashVisuals = GetNetEntity(visualEntity);
            }
        }

        leash.Comp.Leashed.Add(data);
        Dirty(leash);
    }

    public void RemoveLeash(Entity<LeashedComponent?> leashed, Entity<LeashComponent?> leash, bool breakJoint = true)
    {
        if (_net.IsClient || !Resolve(leashed, ref leashed.Comp))
            return;

        var jointId = leashed.Comp.JointId;
        RemCompDeferred<LeashedComponent>(leashed); // Has to be deferred else the client explodes for some reason

        if (_container.TryGetContainer(leashed, LeashedComponent.VisualsContainerName, out var visualsContainer))
            _container.CleanContainer(visualsContainer);

        if (Resolve(leash, ref leash.Comp, false))
        {
            var leashedData = leash.Comp.Leashed.Where(it => it.JointId == jointId).ToList();
            foreach (var data in leashedData)
                leash.Comp.Leashed.Remove(data);
        }

        if (breakJoint && jointId is not null)
            _joints.RemoveJoint(leash, jointId);

        Dirty(leash);
    }

    /// <summary>
    ///     Sets the desired length of the leash. The actual length will be updated on the next physics tick.
    /// </summary>
    public void SetLeashLength(Entity<LeashComponent> leash, float length)
    {
        leash.Comp.Length = length;
        RefreshJoints(leash);
        _popups.PopupPredicted(Loc.GetString("leash-set-length-popup", ("length", length)), leash.Owner, null);
    }

    /// <summary>
    ///     Refreshes all joints for the specified leash.
    ///     This will remove all obsolete joints, such as those for which CanCreateJoint returns false,
    ///     and re-add all joints that were previously removed for the same reason, but became valid later.
    /// </summary>
    public void RefreshJoints(Entity<LeashComponent> leash)
    {
        foreach (var data in leash.Comp.Leashed)
        {
            if (!TryGetEntity(data.Pulled, out var pulled) || !TryComp<LeashedComponent>(pulled, out var leashed))
                continue;

            var shouldExist = CanCreateJoint(pulled.Value, leash);
            var exists = data.JointId != null;

            if (exists && !shouldExist && TryComp<JointComponent>(pulled, out var jointComp) && jointComp.GetJoints.TryGetValue(data.JointId!, out var joint))
            {
                data.JointId = leashed.JointId = null;
                _joints.RemoveJoint(joint);

                Log.Debug($"Removed obsolete leash joint between {leash.Owner} and {pulled.Value}");
            }
            else if (!exists && shouldExist)
            {
                var jointId = $"leash-joint-{data.Pulled}";
                joint = CreateLeashJoint(jointId, leash, pulled.Value);
                data.JointId = leashed.JointId = jointId;

                Log.Debug($"Added new leash joint between {leash.Owner} and {pulled.Value}");
            }
        }
    }

    #endregion
}
