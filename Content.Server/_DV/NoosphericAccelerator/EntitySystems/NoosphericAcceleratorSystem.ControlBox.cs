using Content.Server._DV.NoosphericAccelerator.Components;
using Content.Server.Power.Components;
using Content.Shared.Database;
using Content.Shared._DV.NoospericAccelerator.Components;
using Robust.Shared.Utility;
using System.Diagnostics;
using Content.Server.Administration.Managers;
using Content.Shared.CCVar;
using Content.Shared.Power;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Server._DV.NoosphericAccelerator.EntitySystems;

public sealed partial class NoosphericAcceleratorSystem
{
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private void InitializeControlBoxSystem()
    {
        SubscribeLocalEvent<NoosphericAcceleratorControlBoxComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<NoosphericAcceleratorControlBoxComponent, ComponentShutdown>(OnComponentShutdown);
        SubscribeLocalEvent<NoosphericAcceleratorControlBoxComponent, PowerChangedEvent>(OnControlBoxPowerChange);
        SubscribeLocalEvent<NoosphericAcceleratorControlBoxComponent, NoosphericAcceleratorSetEnableMessage>(
            OnUISetEnableMessage);
        SubscribeLocalEvent<NoosphericAcceleratorControlBoxComponent, NoosphericAcceleratorSetPowerStateMessage>(
            OnUISetPowerMessage);
        SubscribeLocalEvent<NoosphericAcceleratorControlBoxComponent, NoosphericAcceleratorRescanPartsMessage>(
            OnUIRescanMessage);
    }

    public override void Update(float frameTime)
    {
        var curTime = _gameTiming.CurTime;
        var query = EntityQueryEnumerator<NoosphericAcceleratorControlBoxComponent>();
        while (query.MoveNext(out var uid, out var controller))
        {
            if (controller.Firing && curTime >= controller.NextFire)
                Fire(uid, curTime, controller);
        }
    }

    [Conditional("DEBUG")]
    private void EverythingIsWellToFire(NoosphericAcceleratorControlBoxComponent controller)
    {
        DebugTools.Assert(controller.Powered);
        DebugTools.Assert(controller.SelectedStrength != NoosphericAcceleratorPowerState.Standby);
        DebugTools.Assert(controller.Assembled);
        DebugTools.Assert(EntityManager.EntityExists(controller.PortEmitter));
        DebugTools.Assert(EntityManager.EntityExists(controller.ForeEmitter));
        DebugTools.Assert(EntityManager.EntityExists(controller.StarboardEmitter));
    }

    public void Fire(EntityUid uid, TimeSpan curTime, NoosphericAcceleratorControlBoxComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        comp.LastFire = curTime;
        comp.NextFire = curTime + comp.ChargeTime;

        EverythingIsWellToFire(comp);

        var strength = comp.SelectedStrength;
        FireEmitter(comp.PortEmitter!.Value, strength);
        FireEmitter(comp.ForeEmitter!.Value, strength);
        FireEmitter(comp.StarboardEmitter!.Value, strength);
    }

    public void SwitchOn(EntityUid uid, EntityUid? user = null, NoosphericAcceleratorControlBoxComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        DebugTools.Assert(comp.Assembled);

        if (comp.Enabled || !comp.CanBeEnabled)
            return;

        if (user is { } player)
            _adminLogger.Add(LogType.Action,
                LogImpact.Low,
                $"{ToPrettyString(player):player} has turned {ToPrettyString(uid)} on");

        comp.Enabled = true;
        UpdatePowerDraw(uid, comp);

        if (!TryComp<PowerConsumerComponent>(comp.PowerBox, out var powerConsumer)
            || powerConsumer.ReceivedPower >=
            powerConsumer.DrawRate * NoosphericAcceleratorControlBoxComponent.RequiredPowerRatio)
            PowerOn(uid, comp);

        UpdateUI(uid, comp);
    }

    public void SwitchOff(EntityUid uid, EntityUid? user = null, NoosphericAcceleratorControlBoxComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;
        if (!comp.Enabled)
            return;

        if (user is { } player)
            _adminLogger.Add(LogType.Action,
                LogImpact.Low,
                $"{ToPrettyString(player):player} has turned {ToPrettyString(uid)} off");

        comp.Enabled = false;
        SetStrength(uid, NoosphericAcceleratorPowerState.Standby, user, comp);
        UpdatePowerDraw(uid, comp);
        PowerOff(uid, comp);
        UpdateUI(uid, comp);
    }

    public void PowerOn(EntityUid uid, NoosphericAcceleratorControlBoxComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        DebugTools.Assert(comp.Enabled);
        DebugTools.Assert(comp.Assembled);

        if (comp.Powered)
            return;

        comp.Powered = true;
        UpdatePowerDraw(uid, comp);
        UpdateFiring(uid, comp);
        UpdatePartVisualStates(uid, comp);
        UpdateUI(uid, comp);
    }

    public void PowerOff(EntityUid uid, NoosphericAcceleratorControlBoxComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;
        if (!comp.Powered)
            return;

        comp.Powered = false;
        UpdatePowerDraw(uid, comp);
        UpdateFiring(uid, comp);
        UpdatePartVisualStates(uid, comp);
        UpdateUI(uid, comp);
    }

    public void SetStrength(EntityUid uid,
        NoosphericAcceleratorPowerState strength,
        EntityUid? user = null,
        NoosphericAcceleratorControlBoxComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;
        if (comp.StrengthLocked)
            return;

        strength = (NoosphericAcceleratorPowerState)MathHelper.Clamp(
            (int)strength,
            (int)NoosphericAcceleratorPowerState.Standby,
            (int)comp.MaxStrength
        );

        if (strength == comp.SelectedStrength)
            return;

        if (user is { } player)
        {
            var impact = strength switch
            {
                NoosphericAcceleratorPowerState.Standby => LogImpact.Low,
                NoosphericAcceleratorPowerState.Level0
                    or NoosphericAcceleratorPowerState.Level1
                    or NoosphericAcceleratorPowerState.Level2 => LogImpact.Medium,
                NoosphericAcceleratorPowerState.Level3 => LogImpact.Extreme,
                _ => throw new IndexOutOfRangeException(nameof(strength)),
            };

            _adminLogger.Add(LogType.Action,
                impact,
                $"{ToPrettyString(player):player} has set the strength of {ToPrettyString(uid)} to {strength}");


            var alertMinPowerState =
                (NoosphericAcceleratorPowerState)_cfg.GetCVar(CCVars.AdminAlertParticleAcceleratorMinPowerState);
            if (strength >= alertMinPowerState)
            {
                var pos = Transform(uid);
                if (_gameTiming.CurTime > comp.EffectCooldown)
                {
                    _chat.SendAdminAlert(player,
                        Loc.GetString("particle-accelerator-admin-power-strength-warning",
                            ("machine", ToPrettyString(uid)),
                            ("powerState", GetPANumericalLevel(strength)),
                            ("coordinates", pos.Coordinates)));
                    _audio.PlayGlobal("/Audio/Misc/adminlarm.ogg",
                        Filter.Empty().AddPlayers(_adminManager.ActiveAdmins),
                        false,
                        AudioParams.Default.WithVolume(-8f));
                    comp.EffectCooldown = _gameTiming.CurTime + comp.CooldownDuration;
                }
            }
        }

        comp.SelectedStrength = strength;
        UpdateAppearance(uid, comp);
        UpdatePartVisualStates(uid, comp);

        if (comp.Enabled)
        {
            UpdatePowerDraw(uid, comp);
            UpdateFiring(uid, comp);
        }
    }

    private void UpdateFiring(EntityUid uid, NoosphericAcceleratorControlBoxComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        if (!comp.Powered || comp.SelectedStrength < NoosphericAcceleratorPowerState.Level0)
        {
            comp.Firing = false;
            return;
        }

        EverythingIsWellToFire(comp);

        var curTime = _gameTiming.CurTime;
        comp.LastFire = curTime;
        comp.NextFire = curTime + comp.ChargeTime;
        comp.Firing = true;
    }

    private void UpdatePowerDraw(EntityUid uid, NoosphericAcceleratorControlBoxComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;
        if (!TryComp<PowerConsumerComponent>(comp.PowerBox, out var powerConsumer))
            return;

        var powerDraw = comp.BasePowerDraw;
        if (comp.Enabled)
            powerDraw += comp.LevelPowerDraw * (int)comp.SelectedStrength;

        powerConsumer.DrawRate = powerDraw;
    }

    public void UpdateUI(EntityUid uid, NoosphericAcceleratorControlBoxComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        if (!_uiSystem.HasUi(uid, NoosphericAcceleratorControlBoxUiKey.Key))
            return;

        var draw = 0f;
        var receive = 0f;

        if (TryComp<PowerConsumerComponent>(comp.PowerBox, out var powerConsumer))
        {
            draw = powerConsumer.DrawRate;
            receive = powerConsumer.ReceivedPower;
        }

        _uiSystem.SetUiState(uid,
            NoosphericAcceleratorControlBoxUiKey.Key,
            new NoosphericAcceleratorUIState(
                comp.Assembled,
                comp.Enabled,
                comp.SelectedStrength,
                (int)draw,
                (int)receive,
                comp.StarboardEmitter != null,
                comp.ForeEmitter != null,
                comp.PortEmitter != null,
                comp.PowerBox != null,
                comp.FuelChamber != null,
                comp.EndCap != null,
                comp.InterfaceDisabled,
                comp.MaxStrength,
                comp.StrengthLocked
            ));
    }

    private void UpdateAppearance(EntityUid uid,
        NoosphericAcceleratorControlBoxComponent? comp = null,
        AppearanceComponent? appearance = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        _appearanceSystem.SetData(
            uid,
            NoosphericAcceleratorVisuals.VisualState,
            TryComp<ApcPowerReceiverComponent>(uid, out var apcPower) && !apcPower.Powered
                ? NoosphericAcceleratorVisualState.Unpowered
                : (NoosphericAcceleratorVisualState)comp.SelectedStrength,
            appearance
        );
    }

    private void UpdatePartVisualStates(EntityUid uid, NoosphericAcceleratorControlBoxComponent? controller = null)
    {
        if (!Resolve(uid, ref controller))
            return;

        var state = controller.Powered
            ? (NoosphericAcceleratorVisualState)controller.SelectedStrength
            : NoosphericAcceleratorVisualState.Unpowered;

        // UpdatePartVisualState(ControlBox); (We are the control box)
        if (controller.FuelChamber.HasValue)
            _appearanceSystem.SetData(controller.FuelChamber!.Value, NoosphericAcceleratorVisuals.VisualState, state);
        if (controller.PowerBox.HasValue)
            _appearanceSystem.SetData(controller.PowerBox!.Value, NoosphericAcceleratorVisuals.VisualState, state);
        if (controller.PortEmitter.HasValue)
            _appearanceSystem.SetData(controller.PortEmitter!.Value, NoosphericAcceleratorVisuals.VisualState, state);
        if (controller.ForeEmitter.HasValue)
            _appearanceSystem.SetData(controller.ForeEmitter!.Value, NoosphericAcceleratorVisuals.VisualState, state);
        if (controller.StarboardEmitter.HasValue)
            _appearanceSystem.SetData(controller.StarboardEmitter!.Value,
                NoosphericAcceleratorVisuals.VisualState,
                state);
        //no endcap because it has no powerlevel-sprites
    }

    private IEnumerable<EntityUid> AllParts(EntityUid uid, NoosphericAcceleratorControlBoxComponent? comp = null)
    {
        if (Resolve(uid, ref comp))
        {
            if (comp.FuelChamber.HasValue)
                yield return comp.FuelChamber.Value;
            if (comp.EndCap.HasValue)
                yield return comp.EndCap.Value;
            if (comp.PowerBox.HasValue)
                yield return comp.PowerBox.Value;
            if (comp.PortEmitter.HasValue)
                yield return comp.PortEmitter.Value;
            if (comp.ForeEmitter.HasValue)
                yield return comp.ForeEmitter.Value;
            if (comp.StarboardEmitter.HasValue)
                yield return comp.StarboardEmitter.Value;
        }
    }

    private void OnComponentStartup(Entity<NoosphericAcceleratorControlBoxComponent> ent, ref ComponentStartup args)
    {
        if (TryComp<NoosphericAcceleratorPartComponent>(ent, out var part))
            part.Master = ent;
    }

    private void OnComponentShutdown(
        Entity<NoosphericAcceleratorControlBoxComponent> ent,
        ref ComponentShutdown args)
    {
        if (TryComp<NoosphericAcceleratorPartComponent>(ent, out var partStatus))
            partStatus.Master = null;

        var partQuery = GetEntityQuery<NoosphericAcceleratorPartComponent>();
        foreach (var part in AllParts(ent, ent.Comp))
        {
            if (partQuery.TryGetComponent(part, out var partData))
                partData.Master = null;
        }
    }

    // This is the power state for the PA control box itself.
    // Keep in mind that the PA itself can keep firing as long as the HV cable under the power box has... power.
    private void OnControlBoxPowerChange(
        Entity<NoosphericAcceleratorControlBoxComponent> ent,
        ref PowerChangedEvent args)
    {
        UpdateAppearance(ent);

        if (!args.Powered)
            _uiSystem.CloseUi(ent.Owner, NoosphericAcceleratorControlBoxUiKey.Key);
    }

    private void OnUISetEnableMessage(
        Entity<NoosphericAcceleratorControlBoxComponent> ent,
        ref NoosphericAcceleratorSetEnableMessage msg)
    {
        if (!NoosphericAcceleratorControlBoxUiKey.Key.Equals(msg.UiKey))
            return;
        if (ent.Comp.InterfaceDisabled)
            return;
        if (TryComp<ApcPowerReceiverComponent>(ent, out var apcPower) && !apcPower.Powered)
            return;

        if (msg.Enabled)
        {
            if (ent.Comp.Assembled)
                SwitchOn(ent, msg.Actor, ent.Comp);
        }
        else
            SwitchOff(ent, msg.Actor, ent.Comp);

        UpdateUI(ent);
    }

    private void OnUISetPowerMessage(
        Entity<NoosphericAcceleratorControlBoxComponent> ent,
        ref NoosphericAcceleratorSetPowerStateMessage msg)
    {
        if (!NoosphericAcceleratorControlBoxUiKey.Key.Equals(msg.UiKey))
            return;
        if (ent.Comp.InterfaceDisabled)
            return;
        if (TryComp<ApcPowerReceiverComponent>(ent, out var apcPower) && !apcPower.Powered)
            return;

        SetStrength(ent, msg.State, msg.Actor, ent.Comp);

        UpdateUI(ent);
    }

    private void OnUIRescanMessage(
        Entity<NoosphericAcceleratorControlBoxComponent> ent,
        ref NoosphericAcceleratorRescanPartsMessage msg)
    {
        if (!NoosphericAcceleratorControlBoxUiKey.Key.Equals(msg.UiKey))
            return;
        if (ent.Comp.InterfaceDisabled)
            return;
        if (TryComp<ApcPowerReceiverComponent>(ent, out var apcPower) && !apcPower.Powered)
            return;

        RescanParts(ent, msg.Actor, ent.Comp);

        UpdateUI(ent);
    }

    public static int GetPANumericalLevel(NoosphericAcceleratorPowerState state)
    {
        return state switch
        {
            NoosphericAcceleratorPowerState.Level0 => 1,
            NoosphericAcceleratorPowerState.Level1 => 2,
            NoosphericAcceleratorPowerState.Level2 => 3,
            NoosphericAcceleratorPowerState.Level3 => 4,
            _ => 0
        };
    }
}
