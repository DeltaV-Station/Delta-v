using Content.Server._DV.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Server.Store.Systems;
using Content.Shared._DV.Objectives.Systems;
using Content.Shared._DV.Reputation;
using Content.Shared.FixedPoint;

namespace Content.Server._DV.Objectives.Systems;

/// <summary>
/// Handles reputation + TC gains for <see cref="ContractObjectiveComponent"/>.
/// </summary>
public sealed class ContractObjectiveSystem : SharedContractObjectiveSystem
{
    [Dependency] private readonly CodeConditionSystem _codeCondition = default!;
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
        ent.Comp.Pda = args.Pda;

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

    /// <summary>
    /// Fail all active incomplete contracts with a given component, based on a predicate.
    /// </summary>
    public void FailContracts<T>(Predicate<Entity<T>> pred) where T: Component
    {
        var query = EntityQueryEnumerator<T, ContractObjectiveComponent>();
        while (query.MoveNext(out var uid, out var comp, out var contract))
        {
            if (_codeCondition.IsCompleted(uid) || !pred((uid, comp)))
                continue;

            if (contract.Pda is {} pda && TryComp<ContractsComponent>(pda, out var contracts))
                _reputation.TryFailContract((pda, contracts), uid);
        }
    }

    /// <summary>
    /// Look up an objective's stored pda and try to fail it.
    /// </summary>
    public bool TryFailContract(Entity<ContractObjectiveComponent?> objective)
    {
        return Resolve(objective, ref objective.Comp) &&
            objective.Comp.Pda is {} pda &&
            TryComp<ContractsComponent>(pda, out var comp) &&
            _reputation.TryFailContract((pda, comp), objective);
    }

    public override string ContractName(EntityUid objective)
    {
        var title = base.ContractName(objective);
        if (!TryComp<ContractObjectiveComponent>(objective, out var contract))
            return title;

        return $"{title} - {contract.Reputation} REP + {contract.Payment} TC";
    }
}
