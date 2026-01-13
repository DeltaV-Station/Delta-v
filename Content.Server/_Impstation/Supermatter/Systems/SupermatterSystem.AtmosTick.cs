using System.Linq;
using System.Numerics;
using Content.Server.Atmos.Piping.Components;
using Content.Shared._Impstation.Supermatter.Components;
using Content.Shared._Impstation.CCVar;
using Content.Shared.Atmos;
using Content.Shared.Radiation.Components;
using Robust.Shared.Map.Components;

namespace Content.Server._Impstation.Supermatter.Systems;

public sealed partial class SupermatterSystem
{
    /// <summary>
    /// This is used for the radiation levels produced by the supermatter.
    /// </summary>
    private EntityQuery<RadiationSourceComponent> _radiationSourceQuery;

    private void InitializeAtmosTick()
    {
        _radiationSourceQuery = GetEntityQuery<RadiationSourceComponent>();
        
        SubscribeLocalEvent<SupermatterComponent, AtmosDeviceUpdateEvent>(OnAtmosDeviceUpdate);
    }
    
    private void OnAtmosDeviceUpdate(EntityUid uid, SupermatterComponent sm, ref AtmosDeviceUpdateEvent args)
    {
        ProcessAtmos(uid, sm, args.dt);
        HandleDamage(uid, sm); // todo we might be able to predict this, but it would be inaccurate if gasses wildly change from tick to tick
        DirtyFields(uid, sm, MetaData(uid), nameof(SupermatterComponent.Damage), nameof(SupermatterComponent.DamageArchived));
        
        UpdateSupermatterStatus((uid, sm)); // todo predict this only if we want to predict damage, no point otherwise
        
        if (_lightQuery.TryComp(uid, out var light))
            UpdateLight((uid, sm), light);
        
        UpdateAccent((uid, sm));

        if (sm.Power > Config.GetCVar(ImpCCVars.SupermatterPowerPenaltyThreshold) || sm.Damage > sm.DamagePenaltyPoint)
        {
            GenerateAnomalies(uid, sm); // port todo extract to new system
        }
    }

    /// <summary>
    /// Handle power and radiation output depending on atmospheric things.
    /// </summary>
    private void ProcessAtmos(EntityUid uid, SupermatterComponent sm, float frameTime)
    {
        var mix = _atmosphere.GetContainingMixture(uid, true, true);

        if (mix is null)
        {
            sm.GasStorage = null;
            DirtyField(uid, sm, nameof(SupermatterComponent.GasStorage));
            return;
        }

        // Divide the gas efficiency by the grace modifier if the supermatter is unpowered
        var gasEfficiency = GetGasEfficiency((uid, sm));

        // Delta-V - Use RemoveRatio instead of Remove.
        sm.GasStorage = mix.RemoveRatio(gasEfficiency);
        
        var moles = sm.GasStorage.TotalMoles;

        if (!(moles > 0f))
            return;

        var gasComposition = sm.GasStorage.Clone();

        // Let's get the proportions of the gases in the mix for scaling stuff later
        // Delta-V we can multiply by the inverse here instead of dividing 9 times.
        // The slight loss of precision is acceptable considering we're already looking at values between 0 and 1.
        gasComposition.Multiply(1 / moles); 

        // No less then zero, and no greater then one, we use this to do explosions and heat to power transfer.
        var powerRatio = SupermatterGasData.GetPowerMixRatios(gasComposition);

        // Affects plasma, o2 and heat output.
        sm.GasHeatModifier = SupermatterGasData.GetHeatPenalties(gasComposition);
        DirtyField(uid, sm, nameof(SupermatterComponent.GasHeatModifier));
        
        var transmissionBonus = SupermatterGasData.GetTransmitModifiers(gasComposition);

        var h2OBonus = 1 - gasComposition.GetMoles(Gas.WaterVapor) * 0.25f;

        powerRatio = Math.Clamp(powerRatio, 0, 1);
        sm.HeatModifier = Math.Max(sm.GasHeatModifier, 0.5f);
        transmissionBonus *= h2OBonus;

        // Miasma is really just microscopic particulate. It gets consumed like anything else that touches the crystal.
        var ammoniaProportion = gasComposition.GetMoles(Gas.Ammonia);

        if (ammoniaProportion > 0)
        {
            var ammoniaPartialPressure = mix.Pressure * ammoniaProportion;
            var consumedMiasma = Math.Clamp((ammoniaPartialPressure - Config.GetCVar(ImpCCVars.SupermatterAmmoniaConsumptionPressure)) /
                (ammoniaPartialPressure + Config.GetCVar(ImpCCVars.SupermatterAmmoniaPressureScaling)) *
                (1 + powerRatio * Config.GetCVar(ImpCCVars.SupermatterAmmoniaGasMixScaling)),
                0f, 1f);

            consumedMiasma *= ammoniaProportion * moles;

            if (consumedMiasma > 0)
            {
                sm.GasStorage.AdjustMoles(Gas.Ammonia, -consumedMiasma);
                sm.MatterPower += consumedMiasma * Config.GetCVar(ImpCCVars.SupermatterAmmoniaPowerGain);
            }
        }
        
        DirtyField(uid, sm, nameof(SupermatterComponent.GasStorage));

        // Affects the damage heat does to the crystal
        var heatResistance = SupermatterGasData.GetHeatResistances(gasComposition);
        sm.DynamicHeatResistance = Math.Max(heatResistance, 1);

        // More moles of gases are harder to heat than fewer, so let's scale heat damage around them
        sm.MoleHeatPenaltyThreshold = (float)Math.Max(moles / Config.GetCVar(ImpCCVars.SupermatterMoleHeatPenalty), 0.25);

        // Ramps up or down in increments of 0.02 up to the proportion of CO2
        // Given infinite time, powerloss_dynamic_scaling = co2comp
        // Some value from 0-1
        if (moles > Config.GetCVar(ImpCCVars.SupermatterPowerlossInhibitionMoleThreshold) &&
            gasComposition.GetMoles(Gas.CarbonDioxide) > Config.GetCVar(ImpCCVars.SupermatterPowerlossInhibitionGasThreshold))
        {
            var co2powerloss = Math.Clamp(gasComposition.GetMoles(Gas.CarbonDioxide) - sm.PowerlossDynamicScaling, -0.02f, 0.02f);
            sm.PowerlossDynamicScaling = Math.Clamp(sm.PowerlossDynamicScaling + co2powerloss, 0f, 1f);
        }
        else
            sm.PowerlossDynamicScaling = Math.Clamp(sm.PowerlossDynamicScaling - 0.05f, 0f, 1f);

        // Ranges from 0~1(1 - (0~1 * 1~(1.5 * (mol / 150))))
        // We take the mol count, and scale it to be our inhibitor
        sm.PowerlossInhibitor = Math.Clamp(
            1 - sm.PowerlossDynamicScaling * Math.Clamp(moles / Config.GetCVar(ImpCCVars.SupermatterPowerlossInhibitionMoleBoostThreshold), 1f, 1.5f),
            0f, 1f);

        if (sm.MatterPower != 0)
        {
            // We base our removed power off 1/10 the matter_power.
            var removedMatter = Math.Max(sm.MatterPower / Config.GetCVar(ImpCCVars.SupermatterMatterPowerConversion), 40);
            // Adds at least 40 power
            sm.Power = Math.Max(sm.Power + removedMatter, 0);
            // Removes at least 40 matter power
            sm.MatterPower = Math.Max(sm.MatterPower - removedMatter, 0);
        }

        // Based on gas mix, makes the power more based on heat or less effected by heat
        var tempFactor = powerRatio > 0.8 ? 50f : 30f;

        // If there is more frezon and N2 than anything else, we receive no power increase from heat
        sm.Power = Math.Max(sm.GasStorage.Temperature * tempFactor / Atmospherics.T0C * powerRatio + sm.Power, 0);

        // Irradiate stuff
        if (_radiationSourceQuery.TryComp(uid, out var rad))
        {
            rad.Intensity =
                Config.GetCVar(ImpCCVars.SupermatterRadsBase) +
                sm.Power
                * Math.Max(0, 1f + transmissionBonus / 10f)
                * 0.003f
                * Config.GetCVar(ImpCCVars.SupermatterRadsModifier);

            rad.Slope = Math.Clamp(rad.Intensity / 15, 0.2f, 1f);
        }
        
        // Psychological soothing can reduce the energy used for waste gasses and temperatures by up to 20%.
        
        var psyCoefficient = 1f;
        if(PsyReceiversQuery.TryComp(uid, out var psyReceiver))
            psyCoefficient = 1f - psyReceiver.SoothedCurrent * 0.2f;

        // Power * 0.55 * a value between 1 and 0.8
        // This has to be differentiated with respect to time, since its going to be interacting with systems
        // that also differentiate. Basically, if we don't multiply by 2 * frameTime, the supermatter will explode faster if your server's tickrate is higher.
        var energy = sm.Power * Config.GetCVar(ImpCCVars.SupermatterReactionPowerModifier) * psyCoefficient * 2 * frameTime;

        // Keep in mind we are only adding this temperature to (efficiency)% of the one tile the rock is on.
        // An increase of 4°C at 25% efficiency here results in an increase of 1°C / (#tilesincore) overall.
        // Power * 0.55 * 1.5~23 / 5
        var gasReleased = sm.GasStorage.Clone();

        gasReleased.Temperature += energy * sm.HeatModifier / Config.GetCVar(ImpCCVars.SupermatterThermalReleaseModifier);
        gasReleased.Temperature = Math.Max(0,
            Math.Min(gasReleased.Temperature, 2500f * sm.HeatModifier));

        // Release the waste
        gasReleased.AdjustMoles(
            Gas.Plasma,
            Math.Max(energy * sm.HeatModifier / Config.GetCVar(ImpCCVars.SupermatterPlasmaReleaseModifier), 0f));
        gasReleased.AdjustMoles(
            Gas.Oxygen,
            Math.Max((energy + gasReleased.Temperature * sm.HeatModifier - Atmospherics.T0C) / Config.GetCVar(ImpCCVars.SupermatterOxygenReleaseModifier), 0f));
        
        _atmosphere.Merge(mix, gasReleased);

        var powerReduction = (float)Math.Pow(sm.Power / 500, 3);

        // After this point power is lowered
        // This wraps around to the begining of the function
        sm.PowerLoss = Math.Min(powerReduction * sm.PowerlossInhibitor, sm.Power * 0.83f * sm.PowerlossInhibitor);
        sm.Power = Math.Max(sm.Power - sm.PowerLoss, 0f);

        DirtyFields(uid, sm, MetaData(uid), nameof(SupermatterComponent.Power), nameof(SupermatterComponent.PowerLoss));
        
        // Adjust the gravity pull range
        if (_gravityWellQuery.TryComp(uid, out var gravityWell))
            gravityWell.MaxRange = Math.Clamp(sm.Power / 850f, 0.5f, 3f);

        // Log the first powering of the supermatter
        if (sm.Power > 0 && !sm.HasBeenPowered)
            LogFirstPower(uid, sm, mix);
    }

    /// <summary>
    /// Handles environmental damage.
    /// </summary>
    private void HandleDamage(EntityUid uid, SupermatterComponent sm)
    {
        Log.Debug("Server is calling HandleDamage");
        
        var xform = Transform(uid);
        var gridId = xform.GridUid;

        sm.DamageArchived = sm.Damage;

        var mix = _atmosphere.GetContainingMixture(uid, true, true);

        // We're in space or there is no gas to process
        if (!xform.GridUid.HasValue || mix is not { } || MathHelper.CloseTo(mix.TotalMoles, 0f, 0.0005f)) //#IMP change from == 0f to MathHelper.CloseTo(mix.TotalMoles, 0f, 0.0005f)
        {
            var voidDamage = Math.Max(sm.Power / 1000 * sm.DamageIncreaseMultiplier, 0.1f);

            sm.Damage += voidDamage;
            
            var spacingVoidDamageEv = new SupermatterDamagedEvent(voidDamage);
            RaiseLocalEvent(uid, ref spacingVoidDamageEv);
            return;
        }

        // Absorbed gas from surrounding area
        var gasEfficiency = GetGasEfficiency((uid, sm));
        var moles = mix.TotalMoles * gasEfficiency;

        var totalDamage = 0f;

        var tempThreshold = Atmospherics.T0C + Config.GetCVar(ImpCCVars.SupermatterHeatPenaltyThreshold);

        // Temperature start to have a positive effect on damage after 350
        var tempDamage = Math.Max(Math.Clamp(moles / 200f, .5f, 1f) * mix.Temperature - tempThreshold * sm.DynamicHeatResistance, 0f) *
            sm.MoleHeatPenaltyThreshold / 150f * sm.DamageIncreaseMultiplier;
        totalDamage += tempDamage;

        // Power only starts affecting damage when it is above 5000
        var powerDamage = Math.Max(sm.Power - Config.GetCVar(ImpCCVars.SupermatterPowerPenaltyThreshold), 0f) / 500f * sm.DamageIncreaseMultiplier;
        totalDamage += powerDamage;

        // Mol count only starts affecting damage when it is above 1800
        var moleDamage = Math.Max(moles - Config.GetCVar(ImpCCVars.SupermatterMolePenaltyThreshold), 0f) / 80 * sm.DamageIncreaseMultiplier;
        totalDamage += moleDamage;

        // Healing damage
        if (moles < Config.GetCVar(ImpCCVars.SupermatterMolePenaltyThreshold))
        {
            // Only has a net positive effect when the temp is below 313.15, heals up to 2 damage.
            // Psychologists increase this temp min by up to 45 
            var soothingValue = 0f;
            
            if(PsyReceiversQuery.TryComp(uid, out var psyReceiver))
                soothingValue = psyReceiver.SoothedCurrent;
            
            sm.HeatHealing = Math.Min(mix.Temperature - (tempThreshold + 45f * soothingValue), 0f) / 150f;
            totalDamage += sm.HeatHealing;
        }
        else
            sm.HeatHealing = 0f;
        
        DirtyField(uid, sm, nameof(SupermatterComponent.HeatHealing));

        // Check for space tiles next to SM
        if (TryComp<MapGridComponent>(gridId, out var grid))
        {
            var localpos = xform.Coordinates.Position;
            var tilerefs = Map.GetLocalTilesIntersecting(
                gridId.Value,
                grid,
                new Box2(localpos + new Vector2(-1, -1), localpos + new Vector2(1, 1)),
                true);

            // We should have 9 tiles in total, any less means there's a space tile nearby
            if (tilerefs.Count() < 9)
            {
                var factor = GetIntegrity((uid, sm)) switch
                {
                    < 10 => 0.0005f,
                    < 25 => 0.0009f,
                    < 45 => 0.005f,
                    < 75 => 0.002f,
                    _ => 0f
                };

                totalDamage += Math.Clamp(sm.Power * factor * sm.DamageIncreaseMultiplier, 0, sm.MaxSpaceExposureDamage);
            }
        }

        var damage = Math.Min(sm.DamageArchived + sm.MaximumDamagePerCycle * sm.DamageDelaminationThreshold, sm.Damage + totalDamage);
        
        // Prevent it from going negative
        sm.Damage = Math.Clamp(damage, 0, float.PositiveInfinity);
        
        var actualDamage = sm.Damage - sm.DamageArchived;
        
        // Check if the actual damage is close to zero
        if(MathHelper.CloseTo(actualDamage, 0f, float.Epsilon))
            return;

        var damageEv = new SupermatterDamagedEvent(actualDamage);
        RaiseLocalEvent(uid, ref damageEv);
    }
}
