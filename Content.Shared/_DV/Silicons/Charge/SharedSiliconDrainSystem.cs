using Robust.Shared.Random;
using Content.Shared._EE.Silicon.Components;
using Content.Shared.Power.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared._EE.Silicon.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Mind.Components;
using System.Diagnostics.CodeAnalysis;
using Robust.Shared.Timing;
using Robust.Shared.Configuration;
using Robust.Shared.Utility;
using SharedCVars = Content.Shared.CCVar.CCVars;
using Content.Shared.PowerCell.Components;
using Content.Shared.Alert;
using Content.Server._EE.Silicon.Death;
using Content.Server._EE.Power.Components;

namespace Content.Shared._DV.Silicons.Charge;

public abstract class SharedSiliconDrainSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _moveMod = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SiliconComponent, ComponentStartup>(OnSiliconStartup);
    }

    public bool TryGetSiliconBattery(EntityUid silicon, [NotNullWhen(true)] out Entity<BatteryComponent>? batteryComp)
    {
        batteryComp = null;
        if (!HasComp<SiliconComponent>(silicon))
            return false;


        // try get a battery directly on the inserted entity
        if (TryComp<BatteryComponent>(silicon, out var comp))
        {
            batteryComp = (silicon, comp);
            return true;
        }

        if (_powerCell.TryGetBatteryFromSlot(silicon, out batteryComp))
            return true;


        //DebugTools.Assert("SiliconComponent does not contain Battery");
        return false;
    }

    private void OnSiliconStartup(EntityUid uid, SiliconComponent component, ComponentStartup args)
    {
        if (!HasComp<PowerCellSlotComponent>(uid))
            return;

        if (component.EntityType.GetType() != typeof(SiliconType))
            DebugTools.Assert("SiliconComponent.EntityType is not a SiliconType enum.");
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // For each siliconComp entity with a battery component, drain their charge.
        var query = EntityQueryEnumerator<SiliconComponent>();
        while (query.MoveNext(out var silicon, out var siliconComp))
        {
            if (_mobState.IsDead(silicon)
                || !siliconComp.BatteryPowered)
                continue;

            // Check if the Silicon is an NPC, and if so, follow the delay as specified in the CVAR.
            if (siliconComp.EntityType.Equals(SiliconType.Npc))
            {
                var updateTime = _config.GetCVar(SharedCVars.SiliconNpcUpdateTime);
                if (_timing.CurTime - siliconComp.LastDrainTime < TimeSpan.FromSeconds(updateTime))
                    continue;

                siliconComp.LastDrainTime = _timing.CurTime;
            }

            // If you can't find a battery, set the indicator and skip it.
            if (!TryGetSiliconBattery(silicon, out var batteryComp))
            {
                UpdateChargeState(silicon, 0, siliconComp);
                continue;
            }

            // If the silicon ghosted or is SSD while still being powered, skip it.
            if (TryComp<MindContainerComponent>(silicon, out var mindContComp)
                && !mindContComp.HasMind)
                continue;

            var drainRate = siliconComp.DrainPerSecond;

            // All multipliers will be subtracted by 1, and then added together, and then multiplied by the drain rate. This is then added to the base drain rate.
            // This is to stop exponential increases, while still allowing for less-than-one multipliers.
            var drainRateFinalAddi = 0f;

            // TODO: Devise a method of adding multis where other systems can alter the drain rate.
            // Maybe use something similar to refreshmovespeedmodifiers, where it's stored in the component.
            // Maybe it doesn't matter, and stuff should just use static drain?
            if (!siliconComp.EntityType.Equals(SiliconType.Npc)) // Don't bother checking heat if it's an NPC. It's a waste of time, and it'd be delayed due to the update time.
                drainRateFinalAddi += SiliconHeatEffects(silicon, siliconComp, frameTime) - 1; // This will need to be changed at some point if we allow external batteries, since the heat of the Silicon might not be applicable.

            // DeltaV - Sanity check
            if (float.IsNaN(drainRateFinalAddi))
                drainRateFinalAddi = 0f;

            // Ensures that the drain rate is at least 10% of normal,
            // and would allow at least 4 minutes of life with a max charge, to prevent cheese.
            drainRate += Math.Clamp(drainRateFinalAddi, drainRate * -0.9f, batteryComp.Value.Comp.MaxCharge / 240);

            // Drain the battery.
            _powerCell.TryUseCharge(silicon, frameTime * drainRate);

            // Figure out the current state of the Silicon.
            var chargePercent = (short)MathF.Round(_battery.GetCharge(batteryComp.Value.AsNullable()) / batteryComp.Value.Comp.MaxCharge * 10f);

            UpdateChargeState(silicon, chargePercent, siliconComp);
        }
    }

    /// <summary>
    ///Makes sure the entity is showing the right charge alert
    /// </summary>
    /// <param name="ent">The silicon whose charge to check</param>
    /// <param name="chargePercent">Charge level, normalized to range 0-100</param>
    protected abstract void UpdateChargeIcon(Entity<SiliconComponent> ent, short chargePercent);

    /// <summary>
    ///     Checks if anything needs to be updated, and updates it.
    /// </summary>
    public void UpdateChargeState(EntityUid uid, short chargePercent, SiliconComponent component)
    {
        component.ChargeState = chargePercent;

        RaiseLocalEvent(uid, new SiliconChargeStateUpdateEvent(chargePercent));

        _moveMod.RefreshMovementSpeedModifiers(uid);
        UpdateChargeIcon((uid, component), chargePercent);
    }
}
