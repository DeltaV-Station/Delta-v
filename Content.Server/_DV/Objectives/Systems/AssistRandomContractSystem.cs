using Content.Server._DV.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Server.Roles;
using Content.Shared._DV.Reputation;
using Content.Shared.Objectives.Components;
using Content.Shared.Roles;
using Content.Shared.Whitelist;
using Robust.Shared.Random;

namespace Content.Server._DV.Objectives.Systems;

public sealed class AssistRandomContractSystem : EntitySystem
{
    [Dependency] private readonly CodeConditionSystem _codeCondition = default!;
    [Dependency] private readonly ContractObjectiveSystem _contractObjective = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly PickObjectiveTargetSystem _pickTarget = default!;
    [Dependency] private readonly ReputationSystem _reputation = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly TargetObjectiveSystem _target = default!;

    private List<EntityUid> _available = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AssistRandomContractComponent, ObjectiveAssignedEvent>(OnAssigned);
        SubscribeLocalEvent<AssistRandomContractComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
        SubscribeLocalEvent<AssistRandomContractComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<AssistedContractComponent, ContractCompletedEvent>(OnCompleted);
        SubscribeLocalEvent<AssistedContractComponent, ContractFailedEvent>(OnFailed);
        SubscribeLocalEvent<AssistedContractComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnAssigned(Entity<AssistRandomContractComponent> ent, ref ObjectiveAssignedEvent args)
    {
        // have to check contracts against the blacklist
        _pickTarget.AssignRandomTarget(ent, ref args, mindId =>
        {
            // only assist traitor contracts
            if (!_role.MindHasRole<TraitorRoleComponent>(mindId))
                return false;

            // only assist traitors that use contracts
            if (_reputation.GetContracts(mindId) is not {} contracts)
                return false;

            foreach (var obj in contracts.Comp.Objectives)
            {
                if (obj is {} uid && _whitelist.IsBlacklistFailOrNull(ent.Comp.Blacklist, uid))
                    return true; // has at least 1 objective to pick
            }

            return false; // no objectives...
        }, fallbackToAny: false); // don't fallback to non traitors obviously
    }

    private void OnAfterAssign(Entity<AssistRandomContractComponent> ent, ref ObjectiveAfterAssignEvent args)
    {
        if (!_target.GetTarget(ent, out var target))
            return;

        if (_reputation.GetContracts(target) is not {} contracts)
            return;

        _available.Clear();
        foreach (var obj in contracts.Comp.Objectives)
        {
            if (obj is {} uid && _whitelist.IsBlacklistFailOrNull(ent.Comp.Blacklist, uid))
                _available.Add(uid);
        }

        // just incase the target selection fails
        if (_available.Count == 0)
        {
            Log.Error($"Objective {ToPrettyString(ent)} had no available contracts to pick from for {ToPrettyString(target)}!");
            return;
        }

        var contract = _random.Pick(_available);
        ent.Comp.Contract = contract;
        StartAssisting(contract, ent);

        // set description so you know what to do
        var desc = Loc.GetString(ent.Comp.Description, ("contract", Name(contract)));
        _meta.SetEntityDescription(ent, desc, args.Meta);
    }

    private void OnShutdown(Entity<AssistRandomContractComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.Contract is {} contract)
            StopAssisting(contract, ent);
    }

    private void OnCompleted(Entity<AssistedContractComponent> ent, ref ContractCompletedEvent args)
    {
        foreach (var uid in ent.Comp.Assisting)
        {
            _codeCondition.SetCompleted(uid);
        }
    }

    private void OnFailed(Entity<AssistedContractComponent> ent, ref ContractFailedEvent args)
    {
        FailAssisting(ent);
    }

    private void OnShutdown(Entity<AssistedContractComponent> ent, ref ComponentShutdown args)
    {
        FailAssisting(ent);
    }

    public void FailAssisting(Entity<AssistedContractComponent> ent)
    {
        foreach (var uid in ent.Comp.Assisting)
        {
            _contractObjective.TryFailContract(uid);
        }
        ent.Comp.Assisting.Clear();
    }

    public void StartAssisting(EntityUid contract, EntityUid assisting)
    {
        EnsureComp<AssistedContractComponent>(contract).Assisting.Add(assisting);
    }

    public void StopAssisting(EntityUid contract, EntityUid assisting)
    {
        if (!TryComp<AssistedContractComponent>(contract, out var comp))
            return;

        comp.Assisting.Remove(assisting);
        if (comp.Assisting.Count > 0)
            return;

        // nobody is assisting anymore :(
        RemComp<AssistedContractComponent>(contract);
    }
}
