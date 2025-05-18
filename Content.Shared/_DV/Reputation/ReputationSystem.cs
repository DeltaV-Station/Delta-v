using Content.Shared._DV.Objectives.Systems;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Objectives.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._DV.Reputation;

public sealed class ReputationSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedContractObjectiveSystem _contract = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    private List<ReputationLevelPrototype> _levels = new();
    public IReadOnlyList<ReputationLevelPrototype> AllLevels => _levels;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StoreContractsComponent, ComponentInit>(OnStoreInit);
        SubscribeLocalEvent<StoreContractsComponent, ComponentShutdown>(OnStoreShutdown);

        SubscribeLocalEvent<ContractsComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ContractsComponent, EntityUnpausedEvent>(OnUnpaused);
        SubscribeLocalEvent<ContractsComponent, AfterAutoHandleStateEvent>(OnHandleState);
        Subs.BuiEvents<StoreContractsComponent>(ContractsUiKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(OnUIOpened);
            subs.Event<ContractsAcceptMessage>(OnAcceptMessage);
            subs.Event<ContractsCompleteMessage>(OnCompleteMessage);
            subs.Event<ContractsRejectMessage>(OnRejectMessage);
        });

        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);

        CacheLevels();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_net.IsClient) // only server does the rng
            return;

        var query = EntityQueryEnumerator<ContractsComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            PickOfferings((uid, comp));
        }
    }

    #region Event Handlers

    private void OnStoreInit(Entity<StoreContractsComponent> ent, ref ComponentInit args)
    {
        _ui.SetUi(ent.Owner, ContractsUiKey.Key, new InterfaceData("ContractsBUI"));
    }

    private void OnStoreShutdown(Entity<StoreContractsComponent> ent, ref ComponentShutdown args)
    {
        if (GetContracts(ent.Comp.Mind) is not {} contracts)
            return;

        // if the PDA is cremated or eaten by a singulo or something,
        // delete all the offerings and fail the active contracts
        foreach (var uid in contracts.Comp.Offerings)
        {
            Del(uid);
        }

        foreach (var obj in contracts.Comp.Objectives)
        {
            ContractFailed(contracts, obj);
        }

        // don't try to pay TC to this store now that it's deleted
        contracts.Comp.Stores.Remove(ent.Owner);
    }

    private void OnMapInit(Entity<ContractsComponent> ent, ref MapInitEvent args)
    {
        // creates the slots for fresh pdas
        UpdateLevel(ent);
        PickOfferings(ent);
    }

    private void OnUnpaused(Entity<ContractsComponent> ent, ref EntityUnpausedEvent args)
    {
        for (var i = 0; i < ent.Comp.Slots.Count; i++)
        {
            var slot = ent.Comp.Slots[i];
            slot.NextUnlock += args.PausedTime;
            ent.Comp.Slots[i] = slot;
        }

        for (var i = 0; i < ent.Comp.OfferingSlots.Count; i++)
        {
            var slot = ent.Comp.OfferingSlots[i];
            slot.NextUnlock += args.PausedTime;
            ent.Comp.OfferingSlots[i] = slot;
        }
        Dirty(ent);
    }

    private void OnHandleState(Entity<ContractsComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        // update CurrentLevel for client after server changes it, so UI can use it
        UpdateLevel(ent);
        UpdateUI(ent);
    }

    private void OnUIOpened(Entity<StoreContractsComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateStoreUI(ent);
    }

    private void OnAcceptMessage(Entity<StoreContractsComponent> ent, ref ContractsAcceptMessage args)
    {
        if (GetContracts(ent.Comp.Mind) is {} contracts)
            TryAcceptContract(contracts, args.Index);
    }

    private void OnCompleteMessage(Entity<StoreContractsComponent> ent, ref ContractsCompleteMessage args)
    {
        if (GetContracts(ent.Comp.Mind) is {} contracts)
            TryCompleteContract(contracts, args.Index);
    }

    private void OnRejectMessage(Entity<StoreContractsComponent> ent, ref ContractsRejectMessage args)
    {
        if (GetContracts(ent.Comp.Mind) is {} contracts)
            TryRejectOffering(contracts, args.Index);
    }

    #endregion

    #region Public API

    /// <summary>
    /// Add contracts to a traitor's mind and PDA.
    /// Throws if you call this multiple times on the same mind or pda.
    /// </summary>
    public void AddContracts(EntityUid mob, EntityUid? pda)
    {
        if (_mind.GetMind(mob) is not {} mindId)
            return;

        // AddComp so it will throw if you are trying to bulldoze a used mind or pda
        var contracts = AddComp<ContractsComponent>(mindId);
        PickOfferings((mindId, contracts));

        if (pda is not {} uid)
            return;

        var store = AddComp<StoreContractsComponent>(uid);
        SetStoreMind((uid, store), mindId);
    }

    public void ToggleUI(EntityUid user, EntityUid store)
    {
        _ui.TryToggleUi(store, ContractsUiKey.Key, user);
    }

    private void UpdateStoreUI(EntityUid uid)
    {
        _ui.SetUiState(uid, ContractsUiKey.Key, new ContractsState());
    }

    private void UpdateUI(Entity<ContractsComponent> ent)
    {
        foreach (var store in ent.Comp.Stores)
        {
            UpdateStoreUI(store);
        }
    }

    /// <summary>
    /// Pick new offerings for open offering slots.
    /// </summary>
    public void PickOfferings(Entity<ContractsComponent> ent)
    {
        if (!TryComp<MindComponent>(ent, out var mind))
            return;

        if (ent.Comp.CurrentLevel is not {} level)
            return;

        var difficulty = level.MaxDifficulty;
        var groups = level.OfferingGroups;
        for (var i = 0; i < ent.Comp.OfferingSlots.Count; i++)
        {
            // can't add a new offering yet
            if (ent.Comp.Offerings[i] != null || IsLocked(ent.Comp.OfferingSlots[i].NextUnlock))
                continue;

            if (_objectives.GetRandomObjective(ent.Owner, mind, groups, difficulty) is not {} objective)
            {
                // prevent spinlock
                ent.Comp.OfferingSlots[i] = new OfferingSlot
                {
                    NextUnlock = _timing.CurTime + ent.Comp.AcceptDelay
                };
                Dirty(ent);
                UpdateUI(ent);
                continue;
            }

            ent.Comp.Offerings[i] = objective;
            ent.Comp.OfferingSlots[i] = new OfferingSlot
            {
                Title = _contract.ContractName(objective)
            };
            Dirty(ent);
            UpdateUI(ent);
        }
    }

    /// <summary>
    /// Try to take a new contract by adding an existing objective entity.
    /// </summary>
    public bool TryTakeContract(Entity<ContractsComponent> ent, EntityUid objective)
    {
        if (!TryComp<MindComponent>(ent, out var mind) ||
            FindOpenSlot(ent) is not {} index)
        {
            return false;
        }

        _mind.AddObjective(ent.Owner, mind, objective);

        ent.Comp.Objectives[index] = objective;
        var slot = ent.Comp.Slots[index];
        slot.ObjectiveTitle = _contract.ContractName(objective);
        ent.Comp.Slots[index] = slot;
        Dirty(ent);

        var ev = new ContractTakenEvent(ent, (ent.Owner, mind));
        RaiseLocalEvent(objective, ref ev);
        return true;
    }

    public bool TryAcceptContract(Entity<ContractsComponent> ent, int i)
    {
        if (i < 0 || i >= ent.Comp.Offerings.Count)
            return false;

        if (ent.Comp.Offerings[i] is not {} objective || !TryTakeContract(ent, objective))
            return false;

        ent.Comp.Offerings[i] = null;
        ent.Comp.OfferingSlots[i] = new OfferingSlot
        {
            NextUnlock = _timing.CurTime + ent.Comp.AcceptDelay
        };
        Dirty(ent);
        UpdateUI(ent);
        return true;
    }


    /// <summary>
    /// If a contract's objective is complete, pays out etc and removes it.
    /// </summary>
    public bool TryCompleteContract(Entity<ContractsComponent> ent, int index)
    {
        if (index < 0 ||
            index >= ent.Comp.Slots.Count ||
            ent.Comp.Objectives[index] is not {} objective ||
            !TryComp<MindComponent>(ent, out var mind) ||
            !_objectives.IsCompleted(objective, (ent.Owner, mind)))
        {
            return false;
        }

        var ev = new ContractCompletedEvent(ent);
        RaiseLocalEvent(objective, ref ev);

        ClearSlot(ent, index, ent.Comp.CompleteDelay);
        return true;
    }

    public bool TryRejectOffering(Entity<ContractsComponent> ent, int index)
    {
        if (index < 0 ||
            index >= ent.Comp.OfferingSlots.Count ||
            ent.Comp.Offerings[index] is not {} objective)
        {
            return false;
        }

        ent.Comp.Offerings[index] = null;
        ent.Comp.OfferingSlots[index] = new OfferingSlot
        {
            Title = null,
            NextUnlock = _timing.CurTime + ent.Comp.RejectDelay
        };
        Dirty(ent);
        UpdateUI(ent);
        Del(objective);
        return true;
    }

    /// <summary>
    /// Call this to fail a contract if it becomes impossible to complete.
    /// E.g. trying to steal an item that gets deleted
    /// </summary>
    public bool TryFailContract(Entity<ContractsComponent> ent, EntityUid objective)
    {
        if (FindContract(ent, objective) is not {} index)
            return false;

        ContractFailed(ent, objective);
        ClearSlot(ent, index, ent.Comp.CompleteDelay);
        return true;
    }

    /// <summary>
    /// Get the contracts for a mind, if it exists.
    /// </summary>
    public Entity<ContractsComponent>? GetContracts(EntityUid? mindId)
    {
        if (mindId is not {} mind)
            return null;

        if (!TryComp<ContractsComponent>(mind, out var comp))
            return null;

        return (mind, comp);
    }

    /// <summary>
    /// Gets the reputation for a mind, null if it had no <see cref="ContractsComponent"/>.
    /// </summary>
    public int? GetMindReputation(EntityUid? mindId)
    {
        return GetContracts(mindId)?.Comp.Reputation;
    }

    /// <summary>
    /// Gets the reputation for a store, null if it had no <see cref="StoreContractsComponent"/>.
    /// </summary>
    public int? GetStoreReputation(Entity<StoreContractsComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return null;

        return GetMindReputation(ent.Comp.Mind);
    }

    /// <summary>
    /// Returns true if a store is allowed to purchase an item with some reputation requirement.
    /// </summary>
    public bool CanStorePurchase(EntityUid uid, int? needed)
    {
        if (needed is not { } rep)
            return true; // listing doesn't want reputation

        if (!TryComp<StoreContractsComponent>(uid, out var comp))
            return true; // nukie uplink or surplus

        if (GetStoreReputation((uid, comp)) is not { } reputation)
            return false; // uplink implant in non-traitor, no epic gamer loot

        return reputation >= rep;
    }

    public void SetStoreMind(Entity<StoreContractsComponent> ent, EntityUid? mind)
    {
        if (ent.Comp.Mind == mind)
            return;

        if (GetContracts(ent.Comp.Mind) is {} oldContracts)
            oldContracts.Comp.Stores.Remove(ent.Owner);

        ent.Comp.Mind = mind;
        Dirty(ent);

        if (GetContracts(mind) is {} contracts)
            contracts.Comp.Stores.Add(ent.Owner);
    }

    public bool GiveMindReputation(EntityUid mindId, int amount)
    {
        return amount != 0 &&
            GetContracts(mindId) is {} contracts &&
            GiveReputation(contracts, amount);
    }

    public bool GiveReputation(Entity<ContractsComponent> ent, int amount)
    {
        if (amount == 0)
            return false;

        ent.Comp.Reputation = Math.Clamp(ent.Comp.Reputation + amount, 0, 100);
        Dirty(ent);
        UpdateLevel(ent);
        return true;
    }

    /// <summary>
    /// Gets the level prototype for a given reputation.
    /// </summary>
    public ReputationLevelPrototype? GetLevel(int rep)
    {
        foreach (var proto in _levels)
        {
            if (rep >= proto.Reputation)
                return proto;
        }

        return null;
    }

    #endregion

    private bool IsLocked(TimeSpan? nextUnlock)
    {
        return nextUnlock is {} unlock && _timing.CurTime < unlock;
    }

    private int? FindOpenSlot(Entity<ContractsComponent> ent)
    {
        for (var i = 0; i < ent.Comp.Slots.Count; i++)
        {
            if (ent.Comp.Objectives[i] != null)
                continue;

            if (IsLocked(ent.Comp.Slots[i].NextUnlock))
                continue;

            return i;
        }

        return null;
    }

    private int? FindContract(Entity<ContractsComponent> ent, EntityUid objective)
    {
        for (var i = 0; i < ent.Comp.Slots.Count; i++)
        {
            if (ent.Comp.Objectives[i] == objective)
                return i;
        }

        return null;
    }

    private void ClearSlot(Entity<ContractsComponent> ent, int index, TimeSpan delay)
    {
        // old objective is intentionally not deleted, objective stays in the character menu for your greentextful glory / redtextful shame
        ent.Comp.Objectives[index] = null;
        ent.Comp.Slots[index] = new ContractSlot()
        {
            NextUnlock = _timing.CurTime + delay
        };
        Dirty(ent);
        UpdateUI(ent);
    }

    private void UpdateLevel(Entity<ContractsComponent> ent)
    {
        var old = ent.Comp.CurrentLevel;
        ent.Comp.CurrentLevel = GetLevel(ent.Comp.Reputation);
        UpdateContractSlots(ent);
        UpdateOfferingSlots(ent);
    }

    private void UpdateContractSlots(Entity<ContractsComponent> ent)
    {
        var oldSlots = ent.Comp.Slots.Count;
        var newSlots = ent.Comp.CurrentLevel?.MaxContracts ?? 0;
        if (oldSlots == newSlots)
            return;

        if (newSlots > oldSlots)
        {
            // levelling up, add new slot(s)
            for (var i = oldSlots; i < newSlots; i++)
            {
                ent.Comp.Objectives.Add(null);
                ent.Comp.Slots.Add(new ContractSlot());
            }
        }
        else
        {
            // this should never happen but removing objectives just incase
            for (var i = newSlots; i > oldSlots; i--)
            {
                var j = i - 1;
                var objective = ent.Comp.Objectives[j];
                ContractFailed(ent, objective);
                ent.Comp.Objectives.RemoveAt(j);
                ent.Comp.Slots.RemoveAt(j);
            }
        }
        Dirty(ent);
        UpdateUI(ent);
    }

    private void UpdateOfferingSlots(Entity<ContractsComponent> ent)
    {
        var oldSlots = ent.Comp.OfferingSlots.Count;
        var newSlots = ent.Comp.CurrentLevel?.MaxOfferings ?? 0;
        if (oldSlots == newSlots)
            return;

        if (newSlots > oldSlots)
        {
            // levelling up, add new slot(s)
            for (var i = oldSlots; i < newSlots; i++)
            {
                ent.Comp.Offerings.Add(null);
                ent.Comp.OfferingSlots.Add(new OfferingSlot());
            }
        }
        else
        {
            // this should never happen but removing objectives just incase
            for (var i = newSlots; i > oldSlots; i--)
            {
                var j = i - 1;
                var objective = ent.Comp.Offerings[j];
                Del(objective);
                ent.Comp.Offerings.RemoveAt(j);
                ent.Comp.OfferingSlots.RemoveAt(j);
            }
        }
        Dirty(ent);
        UpdateUI(ent);
    }

    private void ContractFailed(Entity<ContractsComponent> ent, EntityUid? uid)
    {
        if (uid is not {} objective)
            return;

        if (!TryComp<MindComponent>(ent, out var mind))
            return;

        var ev = new ContractFailedEvent(ent);
        RaiseLocalEvent(objective, ref ev);
        _mind.TryRemoveObjective((ent.Owner, mind), objective);
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (!args.WasModified<ReputationLevelPrototype>())
            return;

        CacheLevels();
    }

    private void CacheLevels()
    {
        _levels.Clear();
        foreach (var proto in _proto.EnumeratePrototypes<ReputationLevelPrototype>())
        {
            _levels.Add(proto);
        }
        // sort levels by their reputation requirement, descending
        // this allows GetLevel to work
        _levels.Sort((a, b) => (b.Reputation.CompareTo(a.Reputation)));
    }
}
