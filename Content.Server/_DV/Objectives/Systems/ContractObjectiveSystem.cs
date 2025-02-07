using Content.Server._DV.Objectives.Components;
using Content.Server.Store.Systems;
using Content.Shared._DV.Reputation;
using Content.Shared.FixedPoint;

namespace Content.Server._DV.Objectives.Systems;

/// <summary>
/// Handles reputation + TC gains for <see cref="ContractObjectiveComponent"/>.
/// </summary>
public sealed class ContractObjectiveSystem : EntitySystem
{
    [Dependency] private readonly ReputationSystem _reputation = default!;
    [Dependency] private readonly StoreSystem _store = default!;

    private Dictionary<string, FixedPoint2> _currency = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ContractObjectiveComponent, ContractTakenEvent>(OnTaken);
        SubscribeLocalEvent<ContractObjectiveComponent, ContractCompletedEvent>(OnCompleted);
    }

    private void OnTaken(Entity<ContractObjectiveComponent> ent, ref ContractTakenEvent args)
    {
        if (ent.Comp.Prepaid)
            Pay(ent, args.Pda);
    }

    private void OnCompleted(Entity<ContractObjectiveComponent> ent, ref ContractCompletedEvent args)
    {
        _reputation.GiveReputation(args.Pda, ent.Comp.Reputation);
        if (!ent.Comp.Prepaid)
            Pay(ent, args.Pda);
    }

    private void Pay(Entity<ContractObjectiveComponent> ent, EntityUid pda)
    {
        _currency.Clear();
        _currency[ent.Comp.Currency] = ent.Comp.Payment;
        _store.TryAddCurrency(_currency, pda);
    }
}
