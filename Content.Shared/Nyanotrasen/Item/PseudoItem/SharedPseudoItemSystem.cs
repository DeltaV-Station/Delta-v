using Content.Shared.Actions;
using Content.Shared.Bed.Sleep;
using Content.Shared.DoAfter;
using Content.Shared.Hands;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Item.PseudoItem;
using Content.Shared.Popups;
using Content.Shared.Sprite;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Tag;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;

namespace Content.Shared.Nyanotrasen.Item.PseudoItem;

public abstract partial class SharedPseudoItemSystem : EntitySystem
{
    [Dependency] private readonly SharedStorageSystem _storage = default!;
    [Dependency] private readonly SharedItemSystem _item = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedScaleVisualsSystem _scaleVisuals = default!;

    private readonly ProtoId<TagPrototype> PreventTag = "PreventLabel";
    private readonly EntProtoId SleepActionId = "ActionSleep"; // The action used for sleeping inside bags. Currently uses the default sleep action (same as beds)

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PseudoItemComponent, GetVerbsEvent<InnateVerb>>(AddInsertVerb);
        SubscribeLocalEvent<PseudoItemComponent, EntGotRemovedFromContainerMessage>(OnEntRemoved);
        SubscribeLocalEvent<PseudoItemComponent, GettingPickedUpAttemptEvent>(OnGettingPickedUpAttempt);
        SubscribeLocalEvent<PseudoItemComponent, DropAttemptEvent>(OnDropAttempt);
        SubscribeLocalEvent<PseudoItemComponent, ContainerGettingInsertedAttemptEvent>(OnInsertAttempt);
        SubscribeLocalEvent<PseudoItemComponent, InteractionAttemptEvent>(OnInteractAttempt);
        SubscribeLocalEvent<PseudoItemComponent, PseudoItemInsertDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<PseudoItemComponent, AttackAttemptEvent>(OnAttackAttempt);
    }

    private void AddInsertVerb(EntityUid uid, PseudoItemComponent component, GetVerbsEvent<InnateVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        if (component.Active)
            return;

        if (!TryComp<StorageComponent>(args.Target, out var targetStorage))
            return;

        if (!CheckItemFits((uid, component), (args.Target, targetStorage)))
            return;

        if (Transform(args.Target).ParentUid == uid)
            return;

        InnateVerb verb = new()
        {
            Act = () =>
            {
                TryInsert(args.Target, uid, component, targetStorage);
            },
            Text = Loc.GetString("action-name-insert-self"),
            Priority = 2
        };
        args.Verbs.Add(verb);
    }

    public bool TryInsert(EntityUid storageUid, EntityUid toInsert, PseudoItemComponent component,
        StorageComponent? storage = null)
    {
        if (!Resolve(storageUid, ref storage))
            return false;

        if (!CheckItemFits((toInsert, component), (storageUid, storage)))
            return false;

        // Compute the footprint from the entity's current scale. If it is too big, bail out.
        if (!TryGetScaledShape((toInsert, component), out var shape))
            return false;

        var itemComp = new ItemComponent
            { Size = component.Size, Shape = shape, StoredOffset = component.StoredOffset };
        AddComp(toInsert, itemComp);
        _item.VisualsChanged(toInsert);

        _tag.TryAddTag(toInsert, PreventTag);

        if (!_storage.Insert(storageUid, toInsert, out _, null, storage))
        {
            component.Active = false;
            RemComp<ItemComponent>(toInsert);
            return false;
        }

        // If the storage allows sleeping inside, add the respective action
        if (HasComp<AllowsSleepInsideComponent>(storageUid))
            _actions.AddAction(toInsert, ref component.SleepAction, SleepActionId, toInsert);

        component.Active = true;
        return true;
    }

    private void OnEntRemoved(EntityUid uid, PseudoItemComponent component, EntGotRemovedFromContainerMessage args)
    {
        if (!component.Active)
            return;

        RemComp<ItemComponent>(uid);
        component.Active = false;

        _actions.RemoveAction(uid, component.SleepAction); // Remove sleep action if it was added
    }

    protected virtual void OnGettingPickedUpAttempt(EntityUid uid, PseudoItemComponent component,
        GettingPickedUpAttemptEvent args)
    {
        if (args.User == args.Item)
            return;

        _transform.AttachToGridOrMap(uid);
        args.Cancel();
    }

    private void OnDropAttempt(EntityUid uid, PseudoItemComponent component, DropAttemptEvent args)
    {
        if (component.Active)
            args.Cancel();
    }

    private void OnInsertAttempt(EntityUid uid, PseudoItemComponent component,
        ContainerGettingInsertedAttemptEvent args)
    {
        if (!component.Active)
            return;
        // This hopefully shouldn't trigger, but this is a failsafe just in case so we dont bluespace them cats
        args.Cancel();
    }

    // Prevents moving within the bag :)
    private void OnInteractAttempt(EntityUid uid, PseudoItemComponent component, InteractionAttemptEvent args)
    {
        if (args.Uid == args.Target && component.Active)
            args.Cancelled = true;
    }

    private void OnDoAfter(EntityUid uid, PseudoItemComponent component, DoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Args.Used == null)
            return;

        args.Handled = TryInsert(args.Args.Used.Value, uid, component);
    }

    protected void StartInsertDoAfter(EntityUid inserter, EntityUid toInsert, EntityUid storageEntity,
        PseudoItemComponent? pseudoItem = null)
    {
        if (!Resolve(toInsert, ref pseudoItem))
            return;

        var ev = new PseudoItemInsertDoAfterEvent();
        var args = new DoAfterArgs(EntityManager, inserter, 5f, ev, toInsert, toInsert, storageEntity)
        {
            BreakOnMove = true,
            NeedHand = true
        };

        if (_doAfter.TryStartDoAfter(args))
        {
            // Show a popup to the person getting picked up
            _popupSystem.PopupEntity(Loc.GetString("carry-started", ("carrier", inserter)), toInsert, toInsert);
        }
    }

    private void OnAttackAttempt(EntityUid uid, PseudoItemComponent component, AttackAttemptEvent args)
    {
        if (component.Active)
            args.Cancel();
    }

    /// <summary>
    /// The effective scale used to decide whether (and how) the entity fits in storage.
    /// This combines the character's visual scale (species base scale + height slider) with the
    /// per-species <see cref="PseudoItemComponent.SizeMultiplier"/>.
    /// </summary>
    public float GetEffectiveScale(Entity<PseudoItemComponent> ent)
    {
        var visual = _scaleVisuals.GetSpriteScale(ent);
        var averaged = (visual.X + visual.Y) / 2f;
        return averaged * ent.Comp.SizeMultiplier;
    }

    /// <summary>
    /// Computes the stored grid footprint for the entity at its current effective scale.
    /// Returns false when the entity is too big to be stored (effective scale >= MaxDuffelScale),
    /// or when it has no base shape to scale.
    /// </summary>
    public bool TryGetScaledShape(Entity<PseudoItemComponent> ent, out List<Box2i> shape)
    {
        shape = new List<Box2i>();

        if (ent.Comp.Shape is not { } baseShape)
            return false;

        if (GetEffectiveScale(ent) is var scale && scale >= ent.Comp.MaxDuffelScale)
            return false;

        shape = ScaleAndRasterize(baseShape, scale);
        return true;
    }

    /// <summary>
    /// Scales a base grid shape by a factor and re-rasterizes it onto whole grid cells using
    /// nearest-neighbor sampling, returning one 1x1 box per occupied cell.
    /// Looks good enough for most scales. Especially the mins and maxes.
    /// </summary>
    private static List<Box2i> ScaleAndRasterize(IReadOnlyList<Box2i> baseShape, float scale)
    {
        // Collect the filled cells of the base shape and its bounds.
        var filled = new HashSet<Vector2i>();
        int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
        foreach (var box in baseShape)
        {
            for (var x = box.Left; x <= box.Right; x++)
            for (var y = box.Bottom; y <= box.Top; y++)
            {
                filled.Add(new Vector2i(x, y));
                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
            }
        }

        if (filled.Count == 0)
            return new List<Box2i>();

        var baseWidth = maxX - minX + 1;
        var baseHeight = maxY - minY + 1;

        // Target footprint size, rounded up so the largest baggable characters reach the full size.
        var targetWidth = Math.Max(1, (int)MathF.Ceiling(baseWidth * scale));
        var targetHeight = Math.Max(1, (int)MathF.Ceiling(baseHeight * scale));

        var cells = new List<Box2i>();
        for (var tx = 0; tx < targetWidth; tx++)
        for (var ty = 0; ty < targetHeight; ty++)
        {
            // Sample the base cell under the centre of this target cell.
            var sx = minX + Math.Min(baseWidth - 1, (int)((tx + 0.5f) / scale));
            var sy = minY + Math.Min(baseHeight - 1, (int)((ty + 0.5f) / scale));
            if (filled.Contains(new Vector2i(sx, sy)))
                cells.Add(new Box2i(tx, ty, tx, ty));
        }

        return cells;
    }
}
