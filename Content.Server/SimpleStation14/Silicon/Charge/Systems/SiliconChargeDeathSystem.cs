using Content.Server.Power.Components;
using Content.Shared.SimpleStation14.Silicon.Systems;
using Content.Server.Bed.Sleep;
using Content.Shared.Bed.Sleep;
using Content.Server.Sound.Components;
using Content.Server.SimpleStation14.Silicon.Charge;
using System.Threading;
using Content.Server.Humanoid;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Robust.Shared.Utility;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server.SimpleStation14.Silicon.Death;

public sealed class SiliconDeathSystem : EntitySystem
{
    [Dependency] private readonly SleepingSystem _sleep = default!;
    [Dependency] private readonly SiliconChargeSystem _silicon = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoidAppearanceSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SiliconDownOnDeadComponent, SiliconChargeStateUpdateEvent>(OnSiliconChargeStateUpdate);
    }

    private void OnSiliconChargeStateUpdate(EntityUid uid, SiliconDownOnDeadComponent siliconDeadComp, SiliconChargeStateUpdateEvent args)
    {
        if (!_silicon.TryGetSiliconBattery(uid, out var batteryComp))
        {
            SiliconDead(uid, siliconDeadComp, batteryComp, uid);
            return;
        }


        if (args.ChargeState == ChargeState.Dead && siliconDeadComp.Dead)
        {
            siliconDeadComp.WakeToken?.Cancel();
            return;
        }

        if (args.ChargeState == ChargeState.Dead && !siliconDeadComp.Dead)
            SiliconDead(uid, siliconDeadComp, batteryComp, uid);
        else if (args.ChargeState != ChargeState.Dead && siliconDeadComp.Dead)
        {
            if (siliconDeadComp.DeadBuffer > 0)
            {
                siliconDeadComp.WakeToken?.Cancel(); // This should never matter, but better safe than loose timers.

                var wakeToken = new CancellationTokenSource();
                siliconDeadComp.WakeToken = wakeToken;

                // If battery is dead, wait the dead buffer time and then wake it up.
                Timer.Spawn(TimeSpan.FromSeconds(siliconDeadComp.DeadBuffer), () =>
                {
                    if (wakeToken.IsCancellationRequested)
                        return;

                    SiliconUnDead(uid, siliconDeadComp, batteryComp, uid);
                }, wakeToken.Token);
            }
            else
                SiliconUnDead(uid, siliconDeadComp, batteryComp, uid);
        }
    }

    private void SiliconDead(EntityUid uid, SiliconDownOnDeadComponent siliconDeadComp, BatteryComponent? batteryComp, EntityUid batteryUid)
    {
        var deadEvent = new SiliconChargeDyingEvent(uid, batteryComp, batteryUid);
        RaiseLocalEvent(uid, deadEvent);

        if (deadEvent.Cancelled)
            return;

        EntityManager.EnsureComponent<SleepingComponent>(uid);
        EntityManager.EnsureComponent<ForcedSleepingComponent>(uid);

        if (TryComp(uid, out HumanoidAppearanceComponent? humanoidAppearanceComponent))
        {
            var layers = HumanoidVisualLayersExtension.Sublayers(HumanoidVisualLayers.HeadSide);
            _humanoidAppearanceSystem.SetLayersVisibility(uid, layers, false, true, humanoidAppearanceComponent);
        }

        siliconDeadComp.Dead = true;

        RaiseLocalEvent(uid, new SiliconChargeDeathEvent(uid, batteryComp, batteryUid));
    }

    private void SiliconUnDead(EntityUid uid, SiliconDownOnDeadComponent siliconDeadComp, BatteryComponent? batteryComp, EntityUid batteryUid)
    {
        _sleep.TryWaking(uid, null, true);

        siliconDeadComp.Dead = false;

        RaiseLocalEvent(uid, new SiliconChargeAliveEvent(uid, batteryComp, batteryUid));
    }
}

/// <summary>
///     A cancellable event raised when a Silicon is about to go down due to charge.
/// </summary>
/// <remarks>
///     This probably shouldn't be modified unless you intend to fill the Silicon's battery,
///     as otherwise it'll just be triggered again next frame.
/// </remarks>
public sealed class SiliconChargeDyingEvent : CancellableEntityEventArgs
{
    public EntityUid SiliconUid { get; }
    public BatteryComponent? BatteryComp { get; }
    public EntityUid BatteryUid { get; }

    public SiliconChargeDyingEvent(EntityUid siliconUid, BatteryComponent? batteryComp, EntityUid batteryUid)
    {
        SiliconUid = siliconUid;
        BatteryComp = batteryComp;
        BatteryUid = batteryUid;
    }
}

/// <summary>
///     An event raised after a Silicon has gone down due to charge.
/// </summary>
public sealed class SiliconChargeDeathEvent : EntityEventArgs
{
    public EntityUid SiliconUid { get; }
    public BatteryComponent? BatteryComp { get; }
    public EntityUid BatteryUid { get; }

    public SiliconChargeDeathEvent(EntityUid siliconUid, BatteryComponent? batteryComp, EntityUid batteryUid)
    {
        SiliconUid = siliconUid;
        BatteryComp = batteryComp;
        BatteryUid = batteryUid;
    }
}

/// <summary>
///     An event raised after a Silicon has reawoken due to an increase in charge.
/// </summary>
public sealed class SiliconChargeAliveEvent : EntityEventArgs
{
    public EntityUid SiliconUid { get; }
    public BatteryComponent? BatteryComp { get; }
    public EntityUid BatteryUid { get; }

    public SiliconChargeAliveEvent(EntityUid siliconUid, BatteryComponent? batteryComp, EntityUid batteryUid)
    {
        SiliconUid = siliconUid;
        BatteryComp = batteryComp;
        BatteryUid = batteryUid;
    }
}
