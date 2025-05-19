using Content.Server._DV.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Server.Stack;
using Content.Server.Store.Systems;
using Content.Shared._DV.Objectives.Systems;
using Content.Shared._DV.Reputation;
using Content.Shared.FixedPoint;
using Content.Shared.Ghost;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Mind;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Server._DV.Objectives.Systems;

/// <summary>
/// Handles reputation + TC gains for <see cref="ContractObjectiveComponent"/>.
/// </summary>
public sealed class ContractObjectiveSystem : SharedContractObjectiveSystem
{
    [Dependency] private readonly CodeConditionSystem _codeCondition = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ReputationSystem _reputation = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly StackSystem _stack = default!;
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
        ent.Comp.Contracts = args.Contracts;
        if (ent.Comp.Prepaid)
            Pay(ent);
    }

    private void OnCompleted(Entity<ContractObjectiveComponent> ent, ref ContractCompletedEvent args)
    {
        _reputation.GiveReputation(args.Contracts, ent.Comp.Reputation);
        if (!ent.Comp.Prepaid)
            Pay(ent);
    }

    private void Pay(Entity<ContractObjectiveComponent> ent)
    {
        if (_reputation.GetContracts(ent.Comp.Contracts) is not {} contracts)
            return;

        if (contracts.Comp.Store is {} store)
        {
            _currency.Clear();
            _currency[ent.Comp.Currency] = ent.Comp.Payment;
            _store.TryAddCurrency(_currency, store);
            return;
        }

        // try give them TC item there's no store
        var mind = Comp<MindComponent>(contracts);
        // no mob unlucky
        if (mind.OwnedEntity is not {} mob)
            return;

        // don't spawn tc under ghosts, give the dead body TC instead
        if (HasComp<GhostComponent>(mob))
        {
            // cremated, no TC for you!
            if (GetEntity(mind.OriginalOwnedEntity) is not {} original)
                return;

            mob = original;
        }

        if (!Exists(mob))
            return;

        // this is copy pasted from store system because it has no API for spawning cash entities
        var coords = Transform(mob).Coordinates;
        var amountRemaining = ent.Comp.Payment;
        var proto = _proto.Index(ent.Comp.Currency);
        foreach (var value in proto.Cash!.Keys.OrderByDescending(x => x))
        {
            var cashId = proto.Cash[value];
            var amountToSpawn = (int) MathF.Floor((float) (amountRemaining / value));
            var ents = _stack.SpawnMultiple(cashId, amountToSpawn, coords);
            if (ents.FirstOrDefault() is {} cash)
                _hands.PickupOrDrop(mob, cash);
            amountRemaining -= value * amountToSpawn;
        }
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

            if (_reputation.GetContracts(contract.Contracts) is {} contracts)
                _reputation.TryFailContract(contracts, uid);
        }
    }

    /// <summary>
    /// Look up an objective's stored pda and try to fail it.
    /// </summary>
    public bool TryFailContract(Entity<ContractObjectiveComponent?> objective)
    {
        return Resolve(objective, ref objective.Comp) &&
            _reputation.GetContracts(objective.Comp.Contracts) is {} contracts &&
            _reputation.TryFailContract(contracts, objective);
    }

    public override string ContractName(EntityUid objective)
    {
        var title = base.ContractName(objective);
        if (!TryComp<ContractObjectiveComponent>(objective, out var contract))
            return title;

        return $"{title} - {contract.Reputation} REP + {contract.Payment} TC";
    }
}
