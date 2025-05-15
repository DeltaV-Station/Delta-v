using Content.Shared._DV.Reputation;
using Content.Shared.Actions;
using Content.Shared.Implants;
using Content.Shared.Mind;

namespace Content.Shared._DV.Implants;

/// <summary>
/// Assigns an uplink implant's contracts mind to the contracts mind.
/// </summary>
public sealed class UplinkImplantSystem : EntitySystem
{
    [Dependency] private readonly ReputationSystem _reputation = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StoreContractsComponent, ImplantImplantedEvent>(OnImplanted);
        SubscribeLocalEvent<StoreContractsComponent, ImplantRemovedEvent>(OnRemoved);
    }

    private void OnImplanted(Entity<StoreContractsComponent> ent, ref ImplantImplantedEvent args)
    {
        if (args.Implanted is not {} mob)
            return;

        // don't overwrite if the mind is valid
        if (_reputation.GetContracts(ent.Comp.Mind) != null)
            return;

        // implanting into SSD people won't let them use contracts but whatever
        if (_mind.GetMind(mob) is not {} mind)
            return;

        // giving non-traitors an uplink implant won't let them buy rep-gated gear
        if (_reputation.GetContracts(mind) is not {} contracts)
            return;

        _actions.AddAction(mob, ref ent.Comp.ActionId, ent.Comp.Action, ent.Owner);
        _reputation.SetStoreMind(ent, contracts);
    }

    private void OnRemoved(Entity<StoreContractsComponent> ent, ref ImplantRemovedEvent args)
    {
        if (args.Implanted is {} mob)
            _actions.RemoveProvidedActions(mob, ent.Owner);
        _reputation.SetStoreMind(ent, null);
    }
}
