using Content.Server._DV.CosmicCult.Components;
using Content.Server.Actions;
using Content.Server.Atmos.Components;
using Content.Server.Bible.Components;
using Content.Server.Popups;
using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._DV.CosmicCult.EntitySystems;

public sealed class CosmicRiftSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    private readonly HashSet<Entity<HumanoidAppearanceComponent>> _humanoids = [];

    private EntityQuery<CosmicCultComponent> _cultistsQuery;
    private EntityQuery<BibleUserComponent> _chaplainsQuery;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CosmicMalignRiftComponent, InteractHandEvent>(OnInteract);
        SubscribeLocalEvent<CosmicMalignRiftComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<CosmicCultComponent, EventAbsorbRiftDoAfter>(OnAbsorbDoAfter);
        SubscribeLocalEvent<CosmicMalignRiftComponent, EventPurgeRiftDoAfter>(OnPurgeDoAfter);
        SubscribeLocalEvent<CosmicMalignRiftComponent, ComponentInit>(OnRiftStarted);

        _cultistsQuery = GetEntityQuery<CosmicCultComponent>();
        _chaplainsQuery = GetEntityQuery<BibleUserComponent>();
    }

    private void OnRiftStarted(Entity<CosmicMalignRiftComponent> ent, ref ComponentInit args)
    {
        ent.Comp.NextPulseTime = _timing.CurTime + _random.Next(ent.Comp.MinPulseTime, ent.Comp.MaxPulseTime);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CosmicMalignRiftComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.NextPulseTime) continue;
            comp.NextPulseTime = _timing.CurTime + _random.Next(comp.MinPulseTime, comp.MaxPulseTime);

            var pos = Transform(uid).Coordinates;
            Spawn(comp.PulseVFX, pos);
            _lookup.GetEntitiesInRange<HumanoidAppearanceComponent>(pos, comp.PulseRange, _humanoids);
            _humanoids.RemoveWhere(target => _chaplainsQuery.HasComp(target) || _cultistsQuery.HasComp(target));
            foreach(var humanoid in _humanoids)
            {
                if (!pos.TryDistance(EntityManager, Transform(humanoid).Coordinates, out var distance)) continue;
                if (!_random.Prob(comp.PulseProb)) continue;
                var damageMultiplier = Math.Clamp(comp.PulseRange / distance, 1, 10); //0.2 damage per second at max distance, up to 2 per second if closer
                var effectDuration = _random.Next(10, 40); //2-8 damage at max distance, 20-80 damage at min distance
                _statusEffects.TryAddStatusEffect<CosmicEntropyDebuffComponent>(humanoid, "EntropicDegen", TimeSpan.FromSeconds(effectDuration), true);
                if (TryComp<CosmicEntropyDebuffComponent>(humanoid, out var debuff)) debuff.Degen = new(){DamageDict = new(){{"Cold", 0.05 * damageMultiplier}, {"Asphyxiation", 0.15 * damageMultiplier}}};
            }
        }
    }

    private void OnInteract(Entity<CosmicMalignRiftComponent> uid, ref InteractHandEvent args)
    {
        if (args.Handled || uid.Comp.Occupied)
        {
            _popup.PopupEntity(Loc.GetString("cosmiccult-rift-inuse"), args.User, args.User);
            return;
        }

        if (HasComp<BibleUserComponent>(args.User))
        {
            _popup.PopupEntity(Loc.GetString("cosmiccult-rift-chaplainoops"), args.User, args.User);
            return;
        }

        if (!TryComp<CosmicCultComponent>(args.User, out var cultist))
        {
            _popup.PopupEntity(Loc.GetString("cosmiccult-rift-invaliduser"), args.User, args.User);
            return;
        }

        if (cultist.CosmicEmpowered)
        {
            _popup.PopupEntity(Loc.GetString("cosmiccult-rift-alreadyempowered"), args.User, args.User);
            return;
        }

        args.Handled = true;
        uid.Comp.Occupied = true;
        _popup.PopupEntity(Loc.GetString("cosmiccult-rift-beginabsorb"), args.User, args.User);
        var doargs = new DoAfterArgs(EntityManager,
            args.User,
            uid.Comp.AbsorbTime,
            new EventAbsorbRiftDoAfter(),
            args.User,
            uid)
        {
            DistanceThreshold = 1.5f, Hidden = true, BreakOnDamage = true, BreakOnHandChange = true, BreakOnMove = true,
            MovementThreshold = 0.5f,
        };
        _doAfter.TryStartDoAfter(doargs);
    }

    private void OnInteractUsing(Entity<CosmicMalignRiftComponent> uid, ref InteractUsingEvent args)
    {
        if (args.Handled || uid.Comp.Occupied)
        {
            _popup.PopupEntity(Loc.GetString("cosmiccult-rift-inuse"), args.User, args.User);
            return;
        }

        if (HasComp<BibleComponent>(args.Used))
        {
            uid.Comp.Occupied = true;
            _popup.PopupEntity(Loc.GetString("cosmiccult-rift-beginpurge"), args.User, args.User);
            var doargs = new DoAfterArgs(EntityManager,
                args.User,
                uid.Comp.BibleTime,
                new EventPurgeRiftDoAfter(),
                uid,
                uid)
            {
                DistanceThreshold = 1.5f, Hidden = false, BreakOnDamage = true, BreakOnDropItem = true,
                BreakOnMove = true, MovementThreshold = 2f,
            };
            _doAfter.TryStartDoAfter(doargs);
        }
        else if (TryComp<CleanseOnUseComponent>(args.Used, out var comp) && comp.CanPurge)
        {
            uid.Comp.Occupied = true;
            _popup.PopupEntity(Loc.GetString("cosmiccult-rift-beginpurge"), args.User, args.User);
            var doargs = new DoAfterArgs(EntityManager,
                args.User,
                uid.Comp.ChaplainTime,
                new EventPurgeRiftDoAfter(),
                uid,
                uid)
            {
                DistanceThreshold = 1.5f, Hidden = false, BreakOnDamage = true, BreakOnDropItem = true,
                BreakOnMove = true, MovementThreshold = 2f,
            };
            _doAfter.TryStartDoAfter(doargs);
        }
    }

    private void OnAbsorbDoAfter(Entity<CosmicCultComponent> uid, ref EventAbsorbRiftDoAfter args)
    {
        var comp = uid.Comp;
        if (args.Args.Target is not { } target || args.Cancelled || args.Handled)
        {
            if (TryComp<CosmicMalignRiftComponent>(args.Args.Target, out var rift))
                rift.Occupied = false;
            return;
        }

        args.Handled = true;
        var tgtpos = Transform(target).Coordinates;
        var actionEnt = _actions.AddAction(uid, uid.Comp.CosmicFragmentationAction);
        Spawn(uid.Comp.AbsorbVFX, tgtpos);
        comp.ActionEntities.Add(actionEnt);
        comp.CosmicFragmentationActionEntity = actionEnt;
        comp.CosmicEmpowered = true;
        comp.CosmicSiphonQuantity = 2;
        comp.CosmicGlareRange = 10;
        comp.CosmicGlareDuration = TimeSpan.FromSeconds(6);
        comp.CosmicGlareStun = TimeSpan.FromSeconds(1);
        comp.CosmicImpositionDuration = TimeSpan.FromSeconds(7.4);
        comp.CosmicBlankDuration = TimeSpan.FromSeconds(26);
        comp.CosmicBlankDelay = TimeSpan.FromSeconds(0.4);
        comp.Respiration = false;
        EnsureComp<PressureImmunityComponent>(args.User);
        EnsureComp<TemperatureImmunityComponent>(args.User);
        _popup.PopupCoordinates(
            Loc.GetString("cosmiccult-rift-absorb", ("NAME", Identity.Entity(args.Args.User, EntityManager))),
            Transform(args.Args.User).Coordinates,
            PopupType.MediumCaution);
        QueueDel(target);
    }

    private void OnPurgeDoAfter(Entity<CosmicMalignRiftComponent> uid, ref EventPurgeRiftDoAfter args)
    {
        if (args.Args.Target == null || args.Cancelled || args.Handled)
        {
            uid.Comp.Occupied = false;
            return;
        }

        args.Handled = true;
        var tgtpos = Transform(uid).Coordinates;
        Spawn(uid.Comp.PurgeVFX, tgtpos);
        _audio.PlayPvs(uid.Comp.PurgeSound, args.User);
        _popup.PopupCoordinates(
            Loc.GetString("cosmiccult-rift-purge", ("NAME", Identity.Entity(args.Args.User, EntityManager))),
            Transform(args.Args.User).Coordinates,
            PopupType.Medium);
        QueueDel(uid);
    }
}
