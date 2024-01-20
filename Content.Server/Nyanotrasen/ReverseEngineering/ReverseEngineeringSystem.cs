using Content.Shared.Containers.ItemSlots;
using Content.Shared.ReverseEngineering;
using Content.Shared.Audio;
using Content.Shared.Examine;
using Content.Server.Research.TechnologyDisk.Components;
using Content.Server.UserInterface;
using Content.Server.Power.Components;
using Content.Server.Construction;
using Content.Server.Popups;
using Robust.Shared.Containers;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using Robust.Shared.Timing;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;

namespace Content.Server.ReverseEngineering;

public sealed class ReverseEngineeringSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly ItemSlotsSystem _slots = default!;
    [Dependency] private readonly SharedAmbientSoundSystem _ambientSoundSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;

    private const string TargetSlot = "target_slot";
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ReverseEngineeringMachineComponent, EntInsertedIntoContainerMessage>(OnEntInserted);
        SubscribeLocalEvent<ReverseEngineeringMachineComponent, EntRemovedFromContainerMessage>(OnEntRemoved);
        SubscribeLocalEvent<ReverseEngineeringMachineComponent, RefreshPartsEvent>(OnRefreshParts);
        SubscribeLocalEvent<ReverseEngineeringMachineComponent, UpgradeExamineEvent>(OnExamineParts);

        SubscribeLocalEvent<ActiveReverseEngineeringMachineComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ActiveReverseEngineeringMachineComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<ReverseEngineeringMachineComponent, ReverseEngineeringMachineScanButtonPressedMessage>(OnScanButtonPressed);
        SubscribeLocalEvent<ReverseEngineeringMachineComponent, ReverseEngineeringMachineSafetyButtonToggledMessage>(OnSafetyButtonToggled);
        SubscribeLocalEvent<ReverseEngineeringMachineComponent, ReverseEngineeringMachineAutoScanButtonToggledMessage>(OnAutoScanButtonToggled);
        SubscribeLocalEvent<ReverseEngineeringMachineComponent, ReverseEngineeringMachineStopButtonPressedMessage>(OnStopButtonPressed);
        SubscribeLocalEvent<ReverseEngineeringMachineComponent, ReverseEngineeringMachineEjectButtonPressedMessage>(OnEjectButtonPressed);

        SubscribeLocalEvent<ReverseEngineeringMachineComponent, PowerChangedEvent>(OnPowerChanged);

        SubscribeLocalEvent<ReverseEngineeringComponent, ExaminedEvent>(OnExamined);

        SubscribeLocalEvent<ReverseEngineeringMachineComponent, BeforeActivatableUIOpenEvent>((e,c,_) => UpdateUserInterface(e,c));

    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var (active, rev) in EntityQuery<ActiveReverseEngineeringMachineComponent, ReverseEngineeringMachineComponent>())
        {
            UpdateUserInterface(rev.Owner, rev);

            if (_timing.CurTime - active.StartTime < rev.AnalysisDuration)
                continue;

            FinishProbe(rev.Owner, rev, active);
        }
    }

    private void OnEntInserted(EntityUid uid, ReverseEngineeringMachineComponent component, EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != TargetSlot || !TryComp<ReverseEngineeringComponent>(args.Entity, out var rev))
            return;

        _slots.SetLock(uid, TargetSlot, true);
        component.CurrentItem = args.Entity;
        component.CurrentItemDifficulty = rev.Difficulty;
        component.CachedMessage = GetReverseEngineeringScanMessage(component);
        UpdateUserInterface(uid, component);

        if (TryComp<AppearanceComponent>(uid, out var appearance))
            _appearanceSystem.SetData(uid, ReverseEngineeringVisuals.ChamberOpen, false, appearance);
    }

    private void OnEntRemoved(EntityUid uid, ReverseEngineeringMachineComponent component, EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != TargetSlot)
            return;

        component.CurrentItem = null;
        component.CurrentItemDifficulty = 0;
        component.Progress = 0;
        CancelProbe(uid, component);

        if (TryComp<AppearanceComponent>(uid, out var appearance))
            _appearanceSystem.SetData(uid, ReverseEngineeringVisuals.ChamberOpen, true, appearance);
    }

    private void OnRefreshParts(EntityUid uid, ReverseEngineeringMachineComponent component, RefreshPartsEvent args)
    {
        var bonusRating = args.PartRatings[component.MachinePartScanBonus];
        var aversionRating = args.PartRatings[component.MachinePartDangerAversionScore];

        component.ScanBonus = (int) bonusRating;
        component.DangerAversionScore = (int) aversionRating;
    }

    private void OnExamineParts(EntityUid uid, ReverseEngineeringMachineComponent component, UpgradeExamineEvent args)
    {
        args.AddNumberUpgrade("reverse-engineering-machine-bonus-upgrade", component.ScanBonus - 1);
        args.AddNumberUpgrade("reverse-engineering-machine-aversion-upgrade", component.DangerAversionScore - 1);
    }

    private void OnStartup(EntityUid uid, ActiveReverseEngineeringMachineComponent component, ComponentStartup args)
    {
        _ambientSoundSystem.SetAmbience(uid, true);
    }

    private void OnShutdown(EntityUid uid,ActiveReverseEngineeringMachineComponent component, ComponentShutdown args)
    {
        _ambientSoundSystem.SetAmbience(uid, false);
    }

    private void OnScanButtonPressed(EntityUid uid, ReverseEngineeringMachineComponent component, ReverseEngineeringMachineScanButtonPressedMessage args)
    {
        if (component.CurrentItem == null)
            return;

        if (HasComp<ActiveReverseEngineeringMachineComponent>(uid))
            return;

        _audio.PlayPvs(component.ClickSound, uid);

        component.CachedMessage = null;
        var activeComp = EnsureComp<ActiveReverseEngineeringMachineComponent>(uid);
        activeComp.StartTime = _timing.CurTime;
        activeComp.Item = component.CurrentItem.Value;
    }

    private void OnSafetyButtonToggled(EntityUid uid, ReverseEngineeringMachineComponent component, ReverseEngineeringMachineSafetyButtonToggledMessage args)
    {
        _audio.PlayPvs(component.ClickSound, uid);

        component.SafetyOn = args.Safety;
        component.CachedMessage = null;
        UpdateUserInterface(uid, component);
    }

    private void OnAutoScanButtonToggled(EntityUid uid, ReverseEngineeringMachineComponent component, ReverseEngineeringMachineAutoScanButtonToggledMessage args)
    {
        _audio.PlayPvs(component.ClickSound, uid);

        component.AutoScan = args.AutoScan;
    }

    private void OnPowerChanged(EntityUid uid, ReverseEngineeringMachineComponent component, ref PowerChangedEvent args)
    {
        if (!args.Powered)
            CancelProbe(uid, component);
    }

    private void OnExamined(EntityUid uid, ReverseEngineeringComponent component, ExaminedEvent args)
    {
        // TODO: Eventually this should probably get shoved into a contextual examine somewhere like health or machine upgrading.
        // And this can be predicted I guess if difficulty becomes read only.
        args.PushMarkup(Loc.GetString("reverse-engineering-examine", ("diff", component.Difficulty)));
    }

    private void OnStopButtonPressed(EntityUid uid, ReverseEngineeringMachineComponent component, ReverseEngineeringMachineStopButtonPressedMessage args)
    {
        _audio.PlayPvs(component.ClickSound, uid);

        CancelProbe(uid, component);
    }

    private void OnEjectButtonPressed(EntityUid uid, ReverseEngineeringMachineComponent component, ReverseEngineeringMachineEjectButtonPressedMessage args)
    {
        _audio.PlayPvs(component.ClickSound, uid);

        Eject(uid, component);
    }

    private void UpdateUserInterface(EntityUid uid, ReverseEngineeringMachineComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!_ui.TryGetUi(uid, ReverseEngineeringMachineUiKey.Key, out var bui))
            return;

        EntityUid? item = component.CurrentItem;
        if (component.CachedMessage == null)
            component.CachedMessage = GetReverseEngineeringScanMessage(component);

        var totalTime = TimeSpan.Zero;
        var scanning = TryComp<ActiveReverseEngineeringMachineComponent>(uid, out var active);
        var canScan = (item != null && !scanning);
        var remaining = active != null ? _timing.CurTime - active.StartTime : TimeSpan.Zero;
        EntityManager.TryGetNetEntity(item, out var netItem);

        var state = new ReverseEngineeringMachineScanUpdateState(netItem, canScan, component.CachedMessage, scanning, component.SafetyOn, component.AutoScan, component.Progress, remaining, component.AnalysisDuration);

        _ui.SetUiState(bui, state);
    }

    private ReverseEngineeringTickResult Roll(ReverseEngineeringMachineComponent component, out int actualRoll)
    {
        int roll = (_random.Next(1, 6) + _random.Next(1, 6) + _random.Next(1, 6));

        roll += component.ScanBonus;

        if (!component.SafetyOn)
            roll += component.DangerBonus;

        roll -= component.CurrentItemDifficulty;

        actualRoll = roll;
        return roll switch
        {
            <= 9 => ReverseEngineeringTickResult.Destruction,
            <= 10 => ReverseEngineeringTickResult.Stagnation,
            <= 12 => ReverseEngineeringTickResult.SuccessMinor,
            <= 15 => ReverseEngineeringTickResult.SuccessAverage,
            <= 17 => ReverseEngineeringTickResult.SuccessMajor,
            _ => ReverseEngineeringTickResult.InstantSuccess
        };
    }

    private void FinishProbe(EntityUid uid, ReverseEngineeringMachineComponent? component = null, ActiveReverseEngineeringMachineComponent? active = null)
    {
        if (!Resolve(uid, ref component, ref active))
            return;

        if (!TryComp<ReverseEngineeringComponent>(component.CurrentItem, out var rev))
        {
            Logger.Error("We somehow scanned a " + component.CurrentItem + " for reverse engineering...");
            return;
        }

        component.CachedMessage = null;

        var result = Roll(component, out var actualRoll);

        if (result == ReverseEngineeringTickResult.Destruction)
        {
            if (!component.SafetyOn && actualRoll + component.DangerAversionScore < 9)
            {
                Del(component.CurrentItem.Value);
                component.CurrentItem = null;
                if (TryComp<AppearanceComponent>(uid, out var appearance))
                    _appearanceSystem.SetData(uid, ReverseEngineeringVisuals.ChamberOpen, true, appearance);
                _slots.SetLock(uid, TargetSlot, false);
                _audio.PlayPvs(component.FailSound1, uid);
                _audio.PlayPvs(component.FailSound2, uid);
                _popupSystem.PopupEntity(Loc.GetString("reverse-engineering-popup-failure", ("machine", uid)), uid, Shared.Popups.PopupType.MediumCaution);
                CancelProbe(uid, component);
            } else
            {
                result = ReverseEngineeringTickResult.Stagnation;
            }
        }

        component.LastResult = result;

        int bonus = 0;

        switch (result)
        {
            case ReverseEngineeringTickResult.Stagnation:
            {
                bonus += 1;
                break;
            }
            case ReverseEngineeringTickResult.SuccessMinor:
            {
                bonus += 10;
                break;
            }
            case ReverseEngineeringTickResult.SuccessAverage:
            {
                bonus += 25;
                break;
            }
            case ReverseEngineeringTickResult.SuccessMajor:
            {
                bonus += 40;
                break;
            }
            case ReverseEngineeringTickResult.InstantSuccess:
            {
                bonus += 100;
                break;
            }
        }

        component.Progress += bonus;
        component.Progress = Math.Clamp(component.Progress, 0, 100);

        if (component.Progress < 100)
        {
            if (component.AutoScan)
            {
                active.StartTime = _timing.CurTime;
            }
            else
            {
                RemComp<ActiveReverseEngineeringMachineComponent>(uid);
            }
        } else
        {
            CreateDisk(uid, component.DiskPrototype, rev.Recipes);
            _audio.PlayPvs(component.SuccessSound, uid);
            if (rev.NewItem == null)
            {
                Eject(uid, component);
            } else
            {
                _slots.SetLock(uid, TargetSlot, false);
                Spawn(rev.NewItem, Transform(uid).Coordinates);
                if (component.CurrentItem != null)
                    Del(component.CurrentItem.Value);
            }
            RemComp<ActiveReverseEngineeringMachineComponent>(uid);
        }

        UpdateUserInterface(uid, component);
    }

    private void CreateDisk(EntityUid uid, string diskPrototype, List<string>? recipes)
    {
        var disk = Spawn(diskPrototype, Transform(uid).Coordinates);

        if (!TryComp<TechnologyDiskComponent>(disk, out var diskComponent))
            return;

        diskComponent.Recipes = recipes;
    }

    private FormattedMessage? GetReverseEngineeringScanMessage(ReverseEngineeringMachineComponent component)
    {
        var msg = new FormattedMessage();

        if (component.CurrentItem == null)
        {
            msg.AddMarkup(Loc.GetString("reverse-engineering-status-ready"));
            return msg;
        }

        msg.AddMarkup(Loc.GetString("reverse-engineering-current-item", ("item", component.CurrentItem.Value)));
        msg.PushNewline();
        msg.PushNewline();

        var analysisScore = component.ScanBonus;
        if (!component.SafetyOn)
            analysisScore += component.DangerBonus;

        msg.AddMarkup(Loc.GetString("reverse-engineering-analysis-score", ("score", analysisScore)));
        msg.PushNewline();
        msg.AddMarkup(Loc.GetString("reverse-engineering-item-difficulty", ("difficulty", component.CurrentItemDifficulty)));
        msg.PushNewline();
        msg.AddMarkup(Loc.GetString("reverse-engineering-progress", ("progress", component.Progress)));
        msg.PushNewline();

        if (component.LastResult != null)
        {
            string lastProbe = string.Empty;

            switch (component.LastResult)
            {
                case ReverseEngineeringTickResult.Destruction:
                    lastProbe = Loc.GetString("reverse-engineering-failure");
                    break;
                case ReverseEngineeringTickResult.Stagnation:
                    lastProbe = Loc.GetString("reverse-engineering-stagnation");
                    break;
                case ReverseEngineeringTickResult.SuccessMinor:
                    lastProbe = Loc.GetString("reverse-engineering-minor");
                    break;
                case ReverseEngineeringTickResult.SuccessAverage:
                    lastProbe = Loc.GetString("reverse-engineering-average");
                    break;
                case ReverseEngineeringTickResult.SuccessMajor:
                    lastProbe = Loc.GetString("reverse-engineering-major");
                    break;
                case ReverseEngineeringTickResult.InstantSuccess:
                    lastProbe = Loc.GetString("reverse-engineering-success");
                    break;
            }

            msg.AddMarkup(Loc.GetString("reverse-engineering-last-attempt-result", ("result", lastProbe)));
        }

        return msg;
    }

    private void Eject(EntityUid uid, ReverseEngineeringMachineComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        _slots.SetLock(uid, TargetSlot, false);
        _slots.TryEject(uid, TargetSlot, null, out var item);
    }

    private void CancelProbe(EntityUid uid, ReverseEngineeringMachineComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.CachedMessage = null;
        component.LastResult = null;
        RemComp<ActiveReverseEngineeringMachineComponent>(uid);
        UpdateUserInterface(uid, component);
    }
}
