using Content.Shared._Shitmed.Body.Events;
using Content.Shared.Body.Components;
using Content.Shared.Body.Events;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Systems;
using Content.Shared.Interaction;
using Content.Shared.PowerCell;

namespace Content.Shared._DV.Augments;

public sealed class AugmentSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;

    private EntityQuery<AugmentComponent> _query;
    private EntityQuery<AugmentPowerCellSlotComponent> _slotQuery;
    private EntityQuery<InstalledAugmentsComponent> _installedQuery;

    public override void Initialize()
    {
        base.Initialize();

        _query = GetEntityQuery<AugmentComponent>();
        _slotQuery = GetEntityQuery<AugmentPowerCellSlotComponent>();
        _installedQuery = GetEntityQuery<InstalledAugmentsComponent>();

        SubscribeLocalEvent<AugmentComponent, MechanismAddedEvent>(OnAugmentAdded);
        SubscribeLocalEvent<AugmentComponent, MechanismRemovedEvent>(OnAugmentRemoved);

        SubscribeLocalEvent<AugmentPowerCellSlotComponent, PowerCellSlotEmptyEvent>(OnCellSlotEmpty);

        SubscribeLocalEvent<InstalledAugmentsComponent, AccessibleOverrideEvent>(OnAccessibleOverride);

        SubscribeLocalEvent<AugmentMechanismComponent, AugmentPowerAvailableEvent>(OnPowerAvailable);
        SubscribeLocalEvent<AugmentMechanismComponent, AugmentPowerLostEvent>(OnPowerLost);
    }

    private void OnAugmentAdded(Entity<AugmentComponent> ent, ref MechanismAddedEvent args)
    {
        var body = args.Body;
        var installed = EnsureComp<InstalledAugmentsComponent>(body);
        installed.InstalledAugments.Add(GetNetEntity(ent));
        Dirty(body, installed);

        UpdateBodyDraw(body);

        if (HasBodyCharge(body))
        {
            var ev = new AugmentPowerAvailableEvent(body);
            RaiseLocalEvent(ent, ref ev);
        }
        else
        {
            var ev = new AugmentPowerLostEvent(body);
            RaiseLocalEvent(ent, ref ev);
        }
    }

    private void OnAugmentRemoved(Entity<AugmentComponent> ent, ref MechanismRemovedEvent args)
    {
        var body = args.Body;
        var ev = new AugmentPowerLostEvent(body);
        RaiseLocalEvent(ent, ref ev);

        if (!_installedQuery.TryComp(body, out var installed))
            return;

        installed.InstalledAugments.Remove(GetNetEntity(ent));
        if (installed.InstalledAugments.Count == 0)
            RemComp<InstalledAugmentsComponent>(body);
        else
            Dirty(body, installed);

        UpdateBodyDraw(body);
    }

    private void OnCellSlotEmpty(Entity<AugmentPowerCellSlotComponent> ent, ref PowerCellSlotEmptyEvent args)
    {
        if (_body.GetBody(ent) is not {} body)
            return;

        // let augments fail when losing power
        var ev = new AugmentPowerLostEvent(body);
        RelayEvent(body, ref ev);
    }

    private void OnAccessibleOverride(Entity<InstalledAugmentsComponent> augment, ref AccessibleOverrideEvent args)
    {
        if (_body.GetBody(args.Target) != args.User)
            return;

        // let the user interact with their installed augments
        args.Handled = true;
        args.Accessible = true;
    }

    private void OnPowerAvailable(Entity<AugmentMechanismComponent> ent, ref AugmentPowerAvailableEvent args)
    {
        _body.TryEnableMechanism(ent.Owner);
    }

    private void OnPowerLost(Entity<AugmentMechanismComponent> ent, ref AugmentPowerLostEvent args)
    {
        _body.TryDisableMechanism(ent.Owner);
    }

    /// <summary>
    /// Relay an event to all installed augments with a certain component.
    /// </summary>
    public void RelayAugmentEvent<C, E>(EntityUid body, EntityQuery<C> query, ref E args)
    where C: IComponent
    where E: notnull
    {
        if (!_installedQuery.TryComp(body, out var comp))
            return;

        foreach (var ent in comp.InstalledAugments)
        {
            var uid = GetEntity(ent);
            if (query.HasComp(uid))
                RaiseLocalEvent(uid, ref args);
        }
    }

    /// <summary>
    /// Relay an event to all augments.
    /// </summary>
    public void RelayEvent<T>(Entity<BodyComponent?> body, ref T args) where T: notnull
    {
        RelayAugmentEvent<AugmentComponent, T>(body, _query, ref args);
    }

    /// <summary>
    /// Relay an event to any power cell slot augments.
    /// </summary>
    public void RelayCellSlotEvent<T>(Entity<BodyComponent?> body, ref T args) where T: notnull
    {
        RelayAugmentEvent<AugmentPowerCellSlotComponent, T>(body, _slotQuery, ref args);
    }

    /// <summary>
    /// Update the draw of any augment power cell slots in a body.
    /// </summary>
    public void UpdateBodyDraw(Entity<BodyComponent?> body)
    {
        foreach (var organ in _body.GetBodyOrganEntityComps<AugmentPowerCellSlotComponent>(body))
        {
            UpdateSlotDraw(organ.Owner, body);
        }
    }

    /// <summary>
    /// Update the draw of any augment power cell slots in an augment's body.
    /// </summary>
    public void UpdateAugmentDraw(EntityUid augment)
    {
        if (_body.GetBody(augment) is {} body)
            UpdateBodyDraw(body);
    }

    /// <summary>
    /// Updates an augment power cell slot's draw to the latest values from augments.
    /// </summary>
    public void UpdateSlotDraw(Entity<PowerCellDrawComponent?> slot, EntityUid? bodyUid = null)
    {
        if (!Resolve(slot, ref slot.Comp))
            return;

        bodyUid ??= _body.GetBody(slot);
        if (bodyUid is not {} body)
            return;

        var ev = new AugmentGetDrawEvent(slot, body);
        RelayEvent(body, ref ev);

        // hasn't changed, do nothing
        if (slot.Comp.DrawRate == ev.Draw)
            return;

        // update it
        slot.Comp.DrawRate = ev.Draw;
        slot.Comp.Enabled = ev.Draw > 0f;
        Dirty(slot, slot.Comp);
    }

    /// <summary>
    /// Returns true if a body has a power cell slot augment with drawing charge.
    /// </summary>
    public bool HasBodyCharge(Entity<BodyComponent?> body)
    {
        foreach (var organ in _body.GetBodyOrganEntityComps<AugmentPowerCellSlotComponent>(body))
        {
            if (HasSlotCharge((organ, organ.Comp1)))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Returns true if a power cell slot has drawing charge.
    /// </summary>
    public bool HasSlotCharge(Entity<AugmentPowerCellSlotComponent?> slot)
    {
        if (!_slotQuery.Resolve(slot, ref slot.Comp))
            return false;

        return slot.Comp.HasCharge;
    }
}
