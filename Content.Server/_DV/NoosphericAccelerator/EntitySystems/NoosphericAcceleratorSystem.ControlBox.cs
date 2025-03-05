using Content.Server._DV.NoosphericAccelerator.Components;
using Content.Server.Power.Components;
using Content.Shared.Database;
using Content.Shared._DV.NoosphericAccelerator.Components;
using Robust.Shared.Utility;
using System.Diagnostics;
using Content.Shared.Power;
using Content.Shared._DV.Noospherics;

namespace Content.Server._DV.NoosphericAccelerator.EntitySystems;

public sealed partial class NoosphericAcceleratorSystem
{
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
        DebugTools.Assert(controller.SelectedStrength.Particles != NoosphericAcceleratorPowerState.Standby());
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

        FireEmitter(comp.PortEmitter!.Value, comp);
        FireEmitter(comp.ForeEmitter!.Value, comp);
        FireEmitter(comp.StarboardEmitter!.Value, comp);
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
        SetStrength(uid, new NoosphericAcceleratorPowerState(), user, comp);
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

        // Clamp strengths of each particle
        foreach (var type in Enum.GetValues<ParticleType>())
        {
            var curPower = strength.Particles[type];
            strength.Particles[type] = (NoosphericAcceleratorPowerLevel)MathHelper.Clamp(
                (int)curPower, (int)NoosphericAcceleratorPowerLevel.Standby, (int)comp.MaxStrength
            );
        }

        if (strength == comp.SelectedStrength)
            return;

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

        if (!comp.Powered || comp.SelectedStrength.AveragePower() < 0f)
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
        {
            powerDraw += comp.LevelPowerDraw * comp.SelectedStrength.AveragePower();
        }

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
                : NoosphericAcceleratorVisualState.Powered,
            appearance
        );
    }

    private void UpdatePartVisualStates(EntityUid uid, NoosphericAcceleratorControlBoxComponent? controller = null)
    {
        if (!Resolve(uid, ref controller))
            return;

        var state = controller.Powered
            ? NoosphericAcceleratorVisualState.Powered
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

    private void OnComponentStartup(EntityUid uid, NoosphericAcceleratorControlBoxComponent comp, ComponentStartup args)
    {
        if (TryComp<NoosphericAcceleratorPartComponent>(uid, out var part))
            part.Master = uid;
    }

    private void OnComponentShutdown(EntityUid uid,
        NoosphericAcceleratorControlBoxComponent comp,
        ComponentShutdown args)
    {
        if (TryComp<NoosphericAcceleratorPartComponent>(uid, out var partStatus))
            partStatus.Master = null;

        var partQuery = GetEntityQuery<NoosphericAcceleratorPartComponent>();
        foreach (var part in AllParts(uid, comp))
        {
            if (partQuery.TryGetComponent(part, out var partData))
                partData.Master = null;
        }
    }

    // This is the power state for the PA control box itself.
    // Keep in mind that the PA itself can keep firing as long as the HV cable under the power box has... power.
    private void OnControlBoxPowerChange(EntityUid uid,
        NoosphericAcceleratorControlBoxComponent comp,
        ref PowerChangedEvent args)
    {
        UpdateAppearance(uid, comp);

        if (!args.Powered)
            _uiSystem.CloseUi(uid, NoosphericAcceleratorControlBoxUiKey.Key);
    }

    private void OnUISetEnableMessage(EntityUid uid,
        NoosphericAcceleratorControlBoxComponent comp,
        NoosphericAcceleratorSetEnableMessage msg)
    {
        if (!NoosphericAcceleratorControlBoxUiKey.Key.Equals(msg.UiKey))
            return;
        if (comp.InterfaceDisabled)
            return;
        if (TryComp<ApcPowerReceiverComponent>(uid, out var apcPower) && !apcPower.Powered)
            return;

        if (msg.Enabled)
        {
            if (comp.Assembled)
                SwitchOn(uid, msg.Actor, comp);
        }
        else
            SwitchOff(uid, msg.Actor, comp);

        UpdateUI(uid, comp);
    }

    private void OnUISetPowerMessage(EntityUid uid,
        NoosphericAcceleratorControlBoxComponent comp,
        NoosphericAcceleratorSetPowerStateMessage msg)
    {
        if (!NoosphericAcceleratorControlBoxUiKey.Key.Equals(msg.UiKey))
            return;
        if (comp.InterfaceDisabled)
            return;
        if (TryComp<ApcPowerReceiverComponent>(uid, out var apcPower) && !apcPower.Powered)
            return;

        SetStrength(uid, msg.State, msg.Actor, comp);

        UpdateUI(uid, comp);
    }

    private void OnUIRescanMessage(EntityUid uid,
        NoosphericAcceleratorControlBoxComponent comp,
        NoosphericAcceleratorRescanPartsMessage msg)
    {
        if (!NoosphericAcceleratorControlBoxUiKey.Key.Equals(msg.UiKey))
            return;
        if (comp.InterfaceDisabled)
            return;
        if (TryComp<ApcPowerReceiverComponent>(uid, out var apcPower) && !apcPower.Powered)
            return;

        RescanParts(uid, msg.Actor, comp);

        UpdateUI(uid, comp);
    }
}
