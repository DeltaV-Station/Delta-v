// SPDX-FileCopyrightText: 2025 Coenx-flex
// SPDX-FileCopyrightText: 2025 Cojoke
// SPDX-FileCopyrightText: 2025 ScyronX
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Server.Body.Systems;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.DoAfter;
using Content.Server.Ghost.Roles;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Medical;
using Content.Server.Medical.Components;
using Content.Server.Nutrition.Components;
using Content.Shared._Mono.CorticalBorer;
using Content.Shared.Administration.Logs;
using Content.Shared.Alert;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Chat; // Einstein Engines - Languages
using Content.Shared.Database;
using Content.Shared.Inventory;
using Content.Shared.MedicalScanner;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Nutrition.Components; // DeltaV - IngestionBlocker in shared
using Content.Shared.Popups;
using Content.Shared.SSDIndicator;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._Mono.CorticalBorer;

public sealed partial class CorticalBorerSystem : SharedCorticalBorerSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly BloodstreamSystem _blood = default!;
    [Dependency] private readonly HealthAnalyzerSystem _analyzer = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
    [Dependency] private readonly ISharedAdminLogManager _admin = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly GhostRoleSystem _ghost  = default!;

    public override void Initialize()
    {
        SubscribeAbilities();

        SubscribeLocalEvent<CorticalBorerComponent, ComponentStartup>(OnStartup);

        SubscribeLocalEvent<CorticalBorerComponent, CorticalBorerDispenserInjectMessage>(OnInjectReagentMessage);
        SubscribeLocalEvent<CorticalBorerComponent, CorticalBorerDispenserSetInjectAmountMessage>(OnSetInjectAmountMessage);

        SubscribeLocalEvent<InventoryComponent, InfestHostAttempt>(OnInfestHostAttempt);
        SubscribeLocalEvent<CorticalBorerComponent, CheckTargetedSpeechEvent>(OnSpeakEvent);

        SubscribeLocalEvent<CorticalBorerComponent, MindRemovedMessage>(OnMindRemoved);
    }

    private void OnStartup(Entity<CorticalBorerComponent> ent, ref ComponentStartup args)
    {
        //add actions
        foreach (var actionId in ent.Comp.InitialCorticalBorerActions)
            _actions.AddAction(ent, actionId);

        _alerts.ShowAlert(ent.Owner, ent.Comp.ChemicalAlert);
        UpdateUiState(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var comp in EntityManager.EntityQuery<CorticalBorerComponent>())
        {
            if (_timing.CurTime < comp.UpdateTimer)
                continue;

            comp.UpdateTimer = _timing.CurTime + TimeSpan.FromSeconds(comp.UpdateCooldown);

            if (comp.Host != null)
                UpdateChems((comp.Owner, comp), comp.ChemicalGenerationRate);
        }

        foreach (var comp in EntityManager.EntityQuery<CorticalBorerInfestedComponent>())
        {
            if (_timing.CurTime >= comp.ControlTimeEnd)
                EndControl(comp.Borer);
        }
    }

    private void OnSpeakEvent(Entity<CorticalBorerComponent> ent, ref CheckTargetedSpeechEvent args)
    {
        //args.ChatTypeIgnore.Add(InGameICChatType.CollectiveMind); // DeltaV - eradicate language code

        if (ent.Comp.Host.HasValue)
        {
            args.Targets.Add(ent);
            args.Targets.Add(ent.Comp.Host.Value);
        }
    }

    public void UpdateChems(Entity<CorticalBorerComponent> ent, int change)
    {
        var (_, comp) = ent;

        if (comp.ChemicalPoints + change >= comp.ChemicalPointCap)
            comp.ChemicalPoints = comp.ChemicalPointCap;
        else if (comp.ChemicalPoints + change <= 0)
            comp.ChemicalPoints = 0;
        else
            comp.ChemicalPoints += change;

        if (comp.ChemicalPoints % comp.UiUpdateInterval == 0)
            UpdateUiState(ent);

        _alerts.ShowAlert(ent.Owner, ent.Comp.ChemicalAlert);

        Dirty(ent);
    }

    public void OnInfestHostAttempt(Entity<InventoryComponent> entity, ref InfestHostAttempt args)
    {
        IngestionBlockerComponent? blocker;

        if (_inventory.TryGetSlotEntity(entity.Owner, "head", out var headUid) &&
            TryComp(headUid, out blocker) &&
            blocker.Enabled)
        {
            args.Blocker = headUid;
            args.Cancel();
        }
    }

    /// <summary>
    /// Attempts to inject the Borer's host with chems
    /// </summary>
    public bool TryInjectHost(Entity<CorticalBorerComponent> ent,
        CorticalBorerChemicalPrototype chemicalPrototype,
        float chemAmount)
    {
        var (uid, comp) = ent;

        // Need a host to inject something
        if (!comp.Host.HasValue)
        {
            _popup.PopupEntity(Loc.GetString("cortical-borer-no-host"), uid, uid, PopupType.Medium);
            return false;
        }

        // Sugar block from injecting stuff
        if (!CanUseAbility(ent, comp.Host.Value))
            return false;

        // Make sure you can even hold the amount of chems you need
        if (chemicalPrototype.Cost > comp.ChemicalPointCap)
        {
            _popup.PopupEntity(Loc.GetString("cortical-borer-not-enough-chem-storage"), uid, uid, PopupType.Medium);
            return false;
        }

        // Make sure you have enough chems
        if (chemicalPrototype.Cost > comp.ChemicalPoints)
        {
            _popup.PopupEntity(Loc.GetString("cortical-borer-not-enough-chem"), uid, uid, PopupType.Medium);
            return false;
        }

        // no injecting things that don't have blood silly
        if (!TryComp<BloodstreamComponent>(comp.Host, out var bloodstream))
            return false;

        var solution = new Solution();
        solution.AddReagent(chemicalPrototype.Reagent, chemAmount);

        // add the chemicals to the bloodstream of the host
        if (!_blood.TryAddToChemicals((comp.Host.Value, bloodstream), solution))
            return false;

        UpdateChems(ent, -((int)chemAmount * chemicalPrototype.Cost));
        return true;
    }

    private void OnInjectReagentMessage(Entity<CorticalBorerComponent> ent, ref CorticalBorerDispenserInjectMessage message)
    {
        CorticalBorerChemicalPrototype? chemProto = null;
        foreach (var chem in _proto.EnumeratePrototypes<CorticalBorerChemicalPrototype>())
        {
            if (chem.Reagent.Equals(message.ChemProtoId))
            {
                chemProto = chem;
                break;
            }
        }

        if (chemProto != null)
            TryInjectHost(ent, chemProto, ent.Comp.InjectAmount);

        UpdateUiState(ent);
    }

    private void OnSetInjectAmountMessage(Entity<CorticalBorerComponent> ent, ref CorticalBorerDispenserSetInjectAmountMessage message)
    {
        ent.Comp.InjectAmount = message.CorticalBorerDispenserDispenseAmount;
        UpdateUiState(ent);
    }

    private List<CorticalBorerDispenserItem> GetAllBorerChemicals(Entity<CorticalBorerComponent> ent)
    {
        var clones = new List<CorticalBorerDispenserItem>();
        foreach (var prototype in _proto.EnumeratePrototypes<CorticalBorerChemicalPrototype>())
        {
            if (!_proto.TryIndex(prototype.Reagent, out ReagentPrototype? proto))
                continue;

            var reagentName = proto.LocalizedName;
            var reagentId = proto.ID;
            var cost = prototype.Cost;
            var amount = ent.Comp.InjectAmount;
            var chems = ent.Comp.ChemicalPoints;
            var color = proto.SubstanceColor;

            clones.Add(new CorticalBorerDispenserItem(reagentName,reagentId, cost, amount, chems, color)); // need color and name
        }

        return clones;
    }

    private void UpdateUiState(Entity<CorticalBorerComponent> ent)
    {
        var chems = GetAllBorerChemicals(ent);

        var state = new CorticalBorerDispenserBoundUserInterfaceState(chems, (int)ent.Comp.InjectAmount);
        _userInterfaceSystem.SetUiState(ent.Owner, CorticalBorerDispenserUiKey.Key, state);
    }

    public bool TryToggleCheckBlood(Entity<CorticalBorerComponent> ent)
    {
        if(!TryComp<UserInterfaceComponent>(ent, out var uic))
            return false;

        if (!TryComp<HealthAnalyzerComponent>(ent, out var health))
            return false;

        _ui.TryToggleUi((ent, uic), HealthAnalyzerUiKey.Key, ent);

        if (health.ScannedEntity is null && ent.Comp.Host.HasValue)
            OpenCheckBlood(ent, uic);

        return true;
    }

    public void OpenCheckBlood(Entity<CorticalBorerComponent> ent, UserInterfaceComponent uic)
    {
        if (!ent.Comp.Host.HasValue)
            return;

        if (!TryComp<HealthAnalyzerComponent>(ent, out var health))
            return;

        if (!_ui.IsUiOpen((ent,uic), HealthAnalyzerUiKey.Key))
            _ui.OpenUi((ent, uic), HealthAnalyzerUiKey.Key, ent);
        _analyzer.BeginAnalyzingEntity((ent, health), ent.Comp.Host.Value);
    }

    public void CloseCheckBlood(Entity<CorticalBorerComponent> ent, UserInterfaceComponent uic)
    {
        if (!ent.Comp.Host.HasValue)
            return;

        if (!TryComp<HealthAnalyzerComponent>(ent, out var health))
            return;

        if(!health.ScannedEntity.HasValue)
            return;

        _ui.CloseUi((ent, uic), HealthAnalyzerUiKey.Key, ent);
        _analyzer.StopAnalyzingEntity((ent, health), health.ScannedEntity.Value);
    }

    public void TakeControlHost(Entity<CorticalBorerComponent> ent, CorticalBorerInfestedComponent infestedComp)
    {
        var (worm, comp) = ent;

        if (comp.Host is not { } host)
            return;

        // make sure they aren't dead, would throw the worm into a ghost mode and just kill em
        if (TryComp<MobStateComponent>(ent.Comp.Host, out var mobState) &&
            mobState.CurrentState == MobState.Dead)
            return;

        if (TryComp<MindContainerComponent>(host, out var mindContainer) &&
            mindContainer.HasMind ||
            HasComp<GhostRoleComponent>(host))
            infestedComp.ControlTimeEnd = _timing.CurTime + comp.ControlDuration;

        if (_mind.TryGetMind(worm, out var wormMind, out _))
            infestedComp.BorerMindId = wormMind;

        if (_mind.TryGetMind(host, out var controledMind, out _))
        {
            infestedComp.OrigininalMindId = controledMind; // set this var here just in case somehow the mind changes from when the infestation started

            // fish head...
            var dummy = Spawn("FoodMeatFish", MapCoordinates.Nullspace);
            _container.Insert(dummy, infestedComp.ControlContainer);

            _mind.TransferTo(controledMind, dummy);
        }
        else
        {
            infestedComp.OrigininalMindId = null;
        }

        comp.ControlingHost = true;
        _mind.TransferTo(wormMind, host);

        if (TryComp<GhostRoleComponent>(worm, out var ghostRole))
            _ghost.UnregisterGhostRole((worm, ghostRole)); // prevent players from taking the worm role once mind isn't in the worm

        // add the end control and vomit egg action
        if (_actions.AddAction(host, "ActionEndControlHost") is {} actionEnd)
            infestedComp.RemoveAbilities.Add(actionEnd);
        if (comp.CanReproduce &&
            infestedComp.ControlTimeEnd != null) // you can't lay eggs with something you can control forever
        {
            if (_actions.AddAction(host, "ActionLayEggHost") is {} actionLay)
                infestedComp.RemoveAbilities.Add(actionLay);
        }

        var str = $"{ToPrettyString(worm)} has taken control over {ToPrettyString(host)}";

        Log.Info(str);
        _admin.Add(LogType.Mind, LogImpact.High, $"{ToPrettyString(worm)} has taken control over {ToPrettyString(host)}");
        _chat.SendAdminAlert(str);
    }

    public void EndControl(Entity<CorticalBorerComponent> worm)
    {
        var (uid, comp) = worm;

        if (comp.Host is not { } host)
            return;

        if (!TryComp<CorticalBorerInfestedComponent>(host, out var infestedComp))
            return;

        // not controlling anyone
        if (!comp.ControlingHost)
            return;

        comp.ControlingHost = false;

        // remove all the actions set to remove
        foreach (var ability in infestedComp.RemoveAbilities)
        {
            _actions.RemoveAction(host, ability);
        }
        infestedComp.RemoveAbilities = new(); // clear out the list

        if (TryComp<GhostRoleComponent>(worm, out var ghostRole))
            _ghost.RegisterGhostRole((worm, ghostRole)); // re-enable the ghost role after you return to the body

        // Return everyone to their own bodies
        if (!TerminatingOrDeleted(infestedComp.BorerMindId))
            _mind.TransferTo(infestedComp.BorerMindId, infestedComp.Borer);
        if (!TerminatingOrDeleted(infestedComp.OrigininalMindId) && infestedComp.OrigininalMindId.HasValue)
            _mind.TransferTo(infestedComp.OrigininalMindId.Value, host);

        infestedComp.ControlTimeEnd = null;
        _container.CleanContainer(infestedComp.ControlContainer);
    }

    private void OnMindRemoved(Entity<CorticalBorerComponent> ent, ref MindRemovedMessage args)
    {
        if (!ent.Comp.ControlingHost)
            TryEjectBorer(ent); // No storing them in hosts if you don't have a soul
    }
}
