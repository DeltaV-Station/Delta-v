using Content.Shared.Audio;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Examine;
using Content.Shared.Nutrition.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared.ReverseEngineering;

public abstract class SharedReverseEngineeringSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] private readonly ItemSlotsSystem _slots = default!;
    [Dependency] private readonly SharedAmbientSoundSystem _ambientSound = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] protected readonly SharedAudioSystem Audio = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ReverseEngineeringComponent, ExaminedEvent>(OnItemExamined);

        SubscribeLocalEvent<ReverseEngineeringMachineComponent, EntInsertedIntoContainerMessage>(OnEntInserted);
        SubscribeLocalEvent<ReverseEngineeringMachineComponent, EntRemovedFromContainerMessage>(OnEntRemoved);
        Subs.BuiEvents<ReverseEngineeringMachineComponent>(ReverseEngineeringMachineUiKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(OnOpened);
            subs.Event<ReverseEngineeringScanMessage>(OnScanPressed);
            subs.Event<ReverseEngineeringSafetyMessage>(OnSafetyToggled);
            subs.Event<ReverseEngineeringAutoScanMessage>(OnAutoScanToggled);
            subs.Event<ReverseEngineeringStopMessage>(OnStopPressed);
            subs.Event<ReverseEngineeringEjectMessage>(OnEjectPressed);
        });

        SubscribeLocalEvent<ActiveReverseEngineeringMachineComponent, ComponentStartup>(OnActiveStartup);
        SubscribeLocalEvent<ActiveReverseEngineeringMachineComponent, ComponentShutdown>(OnActiveShutdown);
    }

    /// <summary>
    /// Returns true if the machine is actively reverse engineering something.
    /// </summary>
    public bool IsActive(EntityUid uid)
    {
        return HasComp<ActiveReverseEngineeringMachineComponent>(uid);
    }

    /// <summary>
    /// Gets the item currently in the machine's target slot.
    /// </summary>
    public EntityUid? GetItem(Entity<ReverseEngineeringMachineComponent> ent)
    {
        if (!_slots.TryGetSlot(ent, ent.Comp.Slot, out var slot))
            return null;

        return slot.Item;
    }

    /// <summary>
    /// Gets the difficulty of the current item, or 0 if there is none.
    /// </summary>
    public int GetDifficulty(Entity<ReverseEngineeringMachineComponent> ent)
    {
        if (GetItem(ent) is not {} item || !TryComp<ReverseEngineeringComponent>(item, out var rev))
            return 0;

        return rev.Difficulty;
    }

    private void OnItemExamined(Entity<ReverseEngineeringComponent> ent, ref ExaminedEvent args)
    {
        // TODO: Eventually this should probably get shoved into a contextual examine somewhere like health or machine upgrading.
        args.PushMarkup(Loc.GetString("reverse-engineering-examine", ("diff", ent.Comp.Difficulty)));
    }

    private void OnEntInserted(Entity<ReverseEngineeringMachineComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        var (uid, comp) = ent;
        if (args.Container.ID != comp.Slot)
            return;

        _slots.SetLock(uid, comp.Slot, true);
        UpdateUI(ent);

        _appearance.SetData(uid, OpenableVisuals.Opened, false);
    }

    private void OnEntRemoved(Entity<ReverseEngineeringMachineComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != ent.Comp.Slot)
            return;

        CancelProbe(ent);

        _appearance.SetData(ent, OpenableVisuals.Opened, true);
    }

    private void OnActiveStartup(Entity<ActiveReverseEngineeringMachineComponent> ent, ref ComponentStartup args)
    {
        _ambientSound.SetAmbience(ent, true);
    }

    private void OnActiveShutdown(Entity<ActiveReverseEngineeringMachineComponent> ent, ref ComponentShutdown args)
    {
        _ambientSound.SetAmbience(ent, false);
    }

    #region UI

    protected void UpdateUI(Entity<ReverseEngineeringMachineComponent> ent)
    {
        var scanMessage = GetScanMessage(ent);
        var state = new ReverseEngineeringMachineState(scanMessage);
        _ui.SetUiState(ent.Owner, ReverseEngineeringMachineUiKey.Key, state);
    }

    private void OnOpened(Entity<ReverseEngineeringMachineComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateUI(ent);
    }

    private void OnScanPressed(Entity<ReverseEngineeringMachineComponent> ent, ref ReverseEngineeringScanMessage args)
    {
        if (!Timing.IsFirstTimePredicted)
            return;

        if (IsActive(ent) || GetItem(ent) == null)
            return;

        var (uid, comp) = ent;
        Audio.PlayPredicted(comp.ClickSound, uid, args.Actor);

        var active = EnsureComp<ActiveReverseEngineeringMachineComponent>(uid);
        StartProbing((uid, comp, active));
    }

    private void OnSafetyToggled(Entity<ReverseEngineeringMachineComponent> ent, ref ReverseEngineeringSafetyMessage args)
    {
        if (!Timing.IsFirstTimePredicted)
            return;

        var (uid, comp) = ent;
        Audio.PlayPredicted(comp.ClickSound, uid, args.Actor);

        comp.SafetyOn = !comp.SafetyOn;
        Dirty(uid, comp);

        UpdateUI(ent);
    }

    private void OnAutoScanToggled(Entity<ReverseEngineeringMachineComponent> ent, ref ReverseEngineeringAutoScanMessage args)
    {
        if (!Timing.IsFirstTimePredicted)
            return;

        var (uid, comp) = ent;
        Audio.PlayPredicted(comp.ClickSound, uid, args.Actor);

        comp.AutoScan = !comp.AutoScan;
        Dirty(uid, comp);

        UpdateUI(ent);
    }

    private void OnStopPressed(Entity<ReverseEngineeringMachineComponent> ent, ref ReverseEngineeringStopMessage args)
    {
        if (!Timing.IsFirstTimePredicted)
            return;

        Audio.PlayPredicted(ent.Comp.ClickSound, ent, args.Actor);

        CancelProbe(ent);
    }

    private void OnEjectPressed(Entity<ReverseEngineeringMachineComponent> ent, ref ReverseEngineeringEjectMessage args)
    {
        if (!Timing.IsFirstTimePredicted)
            return;

        Audio.PlayPredicted(ent.Comp.ClickSound, ent, args.Actor);

        Eject(ent);
    }

    #endregion

    protected void StartProbing(Entity<ReverseEngineeringMachineComponent, ActiveReverseEngineeringMachineComponent> ent)
    {
        ent.Comp2.NextProbe = Timing.CurTime + ent.Comp1.AnalysisDuration;
        Dirty(ent, ent.Comp2);
    }

    protected void CancelProbe(Entity<ReverseEngineeringMachineComponent> ent)
    {
        ent.Comp.LastResult = null;
        Dirty(ent, ent.Comp);
        RemComp<ActiveReverseEngineeringMachineComponent>(ent);

        UpdateUI(ent);
    }

    protected void Eject(Entity<ReverseEngineeringMachineComponent> ent)
    {
        if (!TryComp<ItemSlotsComponent>(ent, out var slots) || !_slots.TryGetSlot(ent, ent.Comp.Slot, out var slot, slots))
            return;

        _slots.SetLock(ent, slot, false, slots);
        _slots.TryEject(ent, slot, user: null, out _);
    }

    private FormattedMessage GetScanMessage(Entity<ReverseEngineeringMachineComponent> ent)
    {
        var msg = new FormattedMessage();
        if (GetItem(ent) is not {} item || !TryComp<ReverseEngineeringComponent>(item, out var rev))
        {
            msg.AddMarkup(Loc.GetString("reverse-engineering-status-ready"));
            return msg;
        }

        var comp = ent.Comp;
        msg.PushMarkup(Loc.GetString("reverse-engineering-current-item", ("item", item)));
        msg.PushNewline();

        var analysisScore = comp.ScanBonus;
        if (!comp.SafetyOn)
            analysisScore += comp.DangerBonus;

        msg.PushMarkup(Loc.GetString("reverse-engineering-analysis-score", ("score", analysisScore)));
        msg.PushMarkup(Loc.GetString("reverse-engineering-item-difficulty", ("difficulty", rev.Difficulty)));
        msg.PushMarkup(Loc.GetString("reverse-engineering-progress", ("progress", rev.Progress)));

        if (comp.LastResult is {} result)
        {
            var lastProbe = Loc.GetString($"reverse-engineering-result-{result}");

            msg.AddMarkup(Loc.GetString("reverse-engineering-last-attempt-result", ("result", lastProbe)));
        }

        return msg;
    }
}
