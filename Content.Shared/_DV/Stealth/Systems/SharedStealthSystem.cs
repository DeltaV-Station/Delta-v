using Content.Shared._DV.Stealth;
using Content.Shared._DV.Stealth.Components;
using Content.Shared.Hands;
using Content.Shared.Inventory;
using Content.Shared.Stealth.Components;
using Robust.Shared.Containers;

namespace Content.Shared.Stealth;

public abstract partial class SharedStealthSystem
{
    private void InitializeDeltaStealthSystem()
    {
        SubscribeLocalEvent<StealthComponent, StealthModifiedEvent>(OnStealthModified);
        SubscribeLocalEvent<PreventStealthComponent, EntGotRemovedFromContainerMessage>(OnRemoval);
        SubscribeLocalEvent<PreventStealthComponent, EntGotInsertedIntoContainerMessage>(OnInsertion);
        SubscribeLocalEvent<PreventStealthComponent, StealthAddedEvent>(OnStealthAdded);
        SubscribeLocalEvent<PreventStealthComponent, HeldRelayedEvent<StealthAddedEvent>>(OnStealthAdded);
        SubscribeLocalEvent<PreventStealthComponent, InventoryRelayedEvent<StealthAddedEvent>>(OnStealthAdded);
    }

    private void OnStealthModified(Entity<StealthComponent> stealth, ref StealthModifiedEvent args)
    {
        var comp = stealth.Comp;

        SetEnabled(stealth.Owner, args.Enabled ?? comp.Enabled, stealth.Comp);
        comp.MaxVisibility = args.MaxVisibility ?? comp.MaxVisibility;
        comp.MinVisibility = args.MinVisibility ?? comp.MinVisibility;

        Dirty(stealth);
    }

    private void OnInsertion(Entity<PreventStealthComponent> stealth, ref EntGotInsertedIntoContainerMessage args)
    {
        var ev = new StealthModifiedEvent(Enabled: false);
        RaiseLocalEvent(args.Container.Owner, ref ev);
    }

    private void OnRemoval(Entity<PreventStealthComponent> stealth, ref EntGotRemovedFromContainerMessage args)
    {
        var ev = new StealthModifiedEvent(Enabled: true);
        RaiseLocalEvent(args.Container.Owner, ref ev);
    }

    private void OnStealthAdded(Entity<PreventStealthComponent> stealth, ref StealthAddedEvent args)
    {
        var ev = new StealthModifiedEvent(Enabled: false);
        RaiseLocalEvent(args.CloakedEntity, ref ev);
    }

    private void OnStealthAdded(Entity<PreventStealthComponent> stealth, ref InventoryRelayedEvent<StealthAddedEvent> args)
    {
        var ev = new StealthModifiedEvent(Enabled: false);
        RaiseLocalEvent(args.Args.CloakedEntity, ref ev);
    }

    private void OnStealthAdded(Entity<PreventStealthComponent> stealth, ref HeldRelayedEvent<StealthAddedEvent> args)
    {
        var ev = new StealthModifiedEvent(Enabled: false);
        RaiseLocalEvent(args.Args.CloakedEntity, ref ev);
    }
}
