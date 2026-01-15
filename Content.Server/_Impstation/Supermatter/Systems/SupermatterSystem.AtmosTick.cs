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
    
    private void OnAtmosDeviceUpdate(Entity<SupermatterComponent> ent, ref AtmosDeviceUpdateEvent args)
    {
        var mix = _atmosphere.GetContainingMixture(ent.Owner, true, true);
        
        ProcessAtmos(ent, mix, args.dt);
        
        HandleDamage(ent, mix);
        DirtyFields(ent, ent.Comp, MetaData(ent), nameof(SupermatterComponent.Damage), nameof(SupermatterComponent.DamageArchived));
        
        UpdateSupermatterStatus(ent);
        
        if (_lightQuery.TryComp(ent, out var light))
            UpdateLight(ent, light);

        UpdateAccent(ent);

        if (ent.Comp.Power > Config.GetCVar(ImpCCVars.SupermatterPowerPenaltyThreshold) || ent.Comp.Damage > ent.Comp.DamagePenaltyPoint)
        {
            GenerateAnomalies(ent, ent.Comp); // port todo extract to new system
        }
    }

    /// <summary>
    /// Handle power and radiation output depending on atmospheric things.
    /// </summary>
    private void ProcessAtmos(Entity<SupermatterComponent> ent, GasMixture? mix, float atmosDeviceTime)
    {
        if (mix is null)
        {
            ent.Comp.GasStorage = null;
            DirtyField(ent.Owner, ent.Comp, nameof(SupermatterComponent.GasStorage));
            return;
        }

        // Divide the gas efficiency by the grace modifier if the supermatter is unpowered
        var gasEfficiency = GetGasEfficiency(ent.AsNullable());

        ent.Comp.GasStorage = mix.RemoveRatio(gasEfficiency); // Delta-V - Use RemoveRatio instead of Remove.
        
        // TotalMoles actually does a horizontal add so we can just store that value instead of recalculating it 5+ times
        var absorbedMoles = ent.Comp.GasStorage.TotalMoles;
        
        if (ent.Comp.GasVoidProportion > 0f)
        {
            // Get the number of moles to remove based on the absorbed moles, and remove it from the mix
            // This is not removed from the gas storage intentionally.
            // It was done this way to match the unintended feature/bug.

            mix.Remove(ent.Comp.GasVoidProportion * absorbedMoles);
        }

        if (absorbedMoles < Atmospherics.GasMinMoles)
            return;

        var gasComposition = ent.Comp.GasStorage.ToDictionary(
            pair => pair.gas,
            pair => Math.Clamp(pair.moles / absorbedMoles, 0f, 1f)
        );

        // No less then zero, and no greater then one, we use this to do explosions and heat to power transfer.
        var powerRatio = Math.Clamp(SupermatterGasData.GetPowerMixRatios(gasComposition), 0.0f, 1.0f);

        if (ent.Comp.GasStorage.GetMoles(Gas.Ammonia) >= Atmospherics.GasMinMoles)
        {
            // Miasma is really just microscopic particulate. It gets consumed like anything else that touches the crystal.
            var matterPower = ConvertMiasmaToMatterPower(ent.Comp.GasStorage, gasComposition, powerRatio, absorbedMoles);
            ent.Comp.MatterPower += matterPower;
        }

        // We're done processing the gas storage now.
        DirtyField(ent.Owner, ent.Comp, nameof(SupermatterComponent.GasStorage));

        // Affects the damage heat does to the crystal
        var heatResistance = SupermatterGasData.GetHeatResistances(gasComposition);
        ent.Comp.DynamicHeatResistance = Math.Max(heatResistance, 1);

        // More moles of gases are harder to heat than fewer, so let's scale heat damage around them
        ent.Comp.MoleHeatPenaltyThreshold = (float)Math.Max(absorbedMoles / Config.GetCVar(ImpCCVars.SupermatterMoleHeatPenalty), 0.25);

        // Ramps up or down in increments of 0.02 up to the proportion of CO2
        // Given infinite time, powerloss_dynamic_scaling = co2comp
        // Some value from 0-1
        if (absorbedMoles > Config.GetCVar(ImpCCVars.SupermatterPowerlossInhibitionMoleThreshold) &&
            gasComposition[Gas.CarbonDioxide] > Config.GetCVar(ImpCCVars.SupermatterPowerlossInhibitionGasThreshold))
        {
            var co2powerloss = Math.Clamp(gasComposition[Gas.CarbonDioxide] - ent.Comp.PowerlossDynamicScaling, -0.02f, 0.02f);
            ent.Comp.PowerlossDynamicScaling = Math.Clamp(ent.Comp.PowerlossDynamicScaling + co2powerloss, 0f, 1f);
        }
        else
            ent.Comp.PowerlossDynamicScaling = Math.Clamp(ent.Comp.PowerlossDynamicScaling - 0.05f, 0f, 1f);

        // Ranges from 0~1(1 - (0~1 * 1~(1.5 * (mol / 150))))
        // We take the mol count, and scale it to be our inhibitor
        ent.Comp.PowerlossInhibitor = Math.Clamp(
            1 - ent.Comp.PowerlossDynamicScaling * Math.Clamp(absorbedMoles / Config.GetCVar(ImpCCVars.SupermatterPowerlossInhibitionMoleBoostThreshold), 1f, 1.5f),
            0f, 1f);

        if (ent.Comp.MatterPower != 0)
        {
            // We base our removed power off 1/10 the matter_power.
            var removedMatter = Math.Max(ent.Comp.MatterPower / Config.GetCVar(ImpCCVars.SupermatterMatterPowerConversion), 40);
            // Adds at least 40 power
            ent.Comp.Power += removedMatter;
            // Removes at least 40 matter power
            ent.Comp.MatterPower = Math.Max(ent.Comp.MatterPower - removedMatter, 0);
        }

        // Based on gas mix, makes the power more based on heat or less effected by heat
        var tempFactor = powerRatio > 0.8 ? 50f : 30f;

        // If there is more frezon and N2 than anything else, we receive no power increase from heat
        ent.Comp.Power += ent.Comp.GasStorage.Temperature * tempFactor / Atmospherics.T0C * powerRatio;
        
        ProcessRadiation(ent, gasComposition);

        // Affects plasma, o2 and heat output.
        ent.Comp.GasWasteModifier = SupermatterGasData.GetMixWastePenalty(gasComposition);
        DirtyField(ent.Owner, ent.Comp, nameof(SupermatterComponent.GasWasteModifier));

        ProcessWaste(ent.Owner, ent.Comp, atmosDeviceTime, mix);

        var powerReduction = (float) Math.Pow(ent.Comp.Power / 500, 3);

        // After this point power is lowered
        // This wraps around to the begining of the function
        ent.Comp.PowerLoss = Math.Min(powerReduction * ent.Comp.PowerlossInhibitor, ent.Comp.Power * 0.83f * ent.Comp.PowerlossInhibitor);
        ent.Comp.Power = Math.Max(ent.Comp.Power - ent.Comp.PowerLoss, 0f);
        DirtyFields(ent.Owner, ent.Comp, MetaData(ent.Owner), nameof(SupermatterComponent.Power), nameof(SupermatterComponent.PowerLoss));

        // Adjust the gravity pull range
        if (_gravityWellQuery.TryComp(ent.Owner, out var gravityWell))
            gravityWell.MaxRange = Math.Clamp(ent.Comp.Power / 850f, 0.5f, 3f);

        // Log the first powering of the supermatter
        if (ent.Comp.Power > 0 && !ent.Comp.HasBeenPowered)
            LogFirstPower(ent.Owner, ent.Comp, mix);
    }

    private void ProcessWaste(EntityUid uid, SupermatterComponent sm, float deltaTime, GasMixture mix)
    {
        if (sm.GasStorage is null)
            return;
        var gasReleased = sm.GasStorage.Clone();
        var wasteModifier = Math.Max(sm.GasWasteModifier, sm.GasWasteModifierMinimum);
        var maxWasteTemperature = 2500f * wasteModifier;
        
        // Psychological soothing can reduce the energy used for waste gasses and temperatures by up to 20%.
        var soothingFactor = 1f;
        if(PsyReceiversQuery.TryComp(uid, out var psyReceiver))
            soothingFactor -= psyReceiver.SoothedCurrent * 0.2f;

        // This has to be differentiated with respect to time, since its going to be interacting with systems
        // that also differentiate. Basically, if we don't multiply by deltaTime, the supermatter will explode faster if your server's tickrate is higher.
        // Note: deltaTime comes from the atmos device update event, so it doesn't need any additional scaling.
        var energy = sm.Power * Config.GetCVar(ImpCCVars.SupermatterReactionPowerModifier) * soothingFactor * deltaTime;

        if (gasReleased.Temperature < maxWasteTemperature)
        {
            // Keep in mind we are only adding this temperature to (efficiency)% of the one tile the rock is on.
            // An increase of 4°C at 25% efficiency here results in an increase of 1°C / (#tilesincore) overall.
            // Power * 0.55 * 1.5~23 / 5
            gasReleased.Temperature += energy * wasteModifier / Config.GetCVar(ImpCCVars.SupermatterThermalReleaseModifier);
            gasReleased.Temperature = Math.Min(gasReleased.Temperature, maxWasteTemperature);
        }

        gasReleased.AdjustMoles(
            Gas.Plasma,
            Math.Max(energy * wasteModifier / Config.GetCVar(ImpCCVars.SupermatterPlasmaReleaseModifier), 0f));
        gasReleased.AdjustMoles(
            Gas.Oxygen,
            Math.Max((energy + gasReleased.Temperature * wasteModifier - Atmospherics.T0C) / Config.GetCVar(ImpCCVars.SupermatterOxygenReleaseModifier), 0f));
        
        // Release the waste
        _atmosphere.Merge(mix, gasReleased);
    }

    private void ProcessRadiation(Entity<SupermatterComponent> ent, Dictionary<Gas, float> gasRatios)
    {
        if (!_radiationSourceQuery.TryComp(ent.Owner, out var rad))
            return;

        var transmissionBonus = SupermatterGasData.GetTransmitModifiers(gasRatios);

        var h2OBonus = 1 - gasRatios[Gas.WaterVapor] * 0.25f;
        transmissionBonus *= h2OBonus;

        // Irradiate stuff

        rad.Intensity =
            ent.Comp.RadsBase +
            ent.Comp.Power * Math.Max(0, 1f + transmissionBonus / 10f) * 0.003f * ent.Comp.RadsModifier;

        rad.Slope = Math.Clamp(rad.Intensity / 15, 0.2f, 1f);
    }

    private float ConvertMiasmaToMatterPower(GasMixture gasStorage, Dictionary<Gas, float> gasRatios, float powerRatio, float totalMoles)
    {
        var ammoniaProportion = gasRatios[Gas.Ammonia];
        var ammoniaPartialPressure = gasStorage.Pressure * ammoniaProportion;

        var consumedMiasma = Math.Clamp((ammoniaPartialPressure - Config.GetCVar(ImpCCVars.SupermatterAmmoniaConsumptionPressure)) /
                                        (ammoniaPartialPressure + Config.GetCVar(ImpCCVars.SupermatterAmmoniaPressureScaling)) *
                                        (1 + powerRatio * Config.GetCVar(ImpCCVars.SupermatterAmmoniaGasMixScaling)),
            0f, 1f);

        consumedMiasma *= ammoniaProportion * totalMoles;

        if (consumedMiasma < Atmospherics.GasMinMoles)
            return 0f;

        gasStorage.AdjustMoles(Gas.Ammonia, -consumedMiasma);
        return consumedMiasma * Config.GetCVar(ImpCCVars.SupermatterAmmoniaPowerGain);
    }

    /// <summary>
    /// Handles environmental damage.
    /// </summary>
    private void HandleDamage(Entity<SupermatterComponent> ent, GasMixture? mix)
    {
        var xform = Transform(ent);
        var gridId = xform.GridUid;

        ent.Comp.DamageArchived = ent.Comp.Damage;

        // We're in space or there is no gas to process
        if (!gridId.HasValue || mix is null || MathHelper.CloseTo(mix.TotalMoles, 0f, 0.0005f)) //#IMP change from == 0f to MathHelper.CloseTo(mix.TotalMoles, 0f, 0.0005f)
        {
            var voidDamage = Math.Max(ent.Comp.Power / 1000 * ent.Comp.DamageMultiplier, 0.1f);

            ent.Comp.Damage += voidDamage;

            var spacingVoidDamageEv = new SupermatterDamagedEvent();
            RaiseLocalEvent(ent, ref spacingVoidDamageEv);
            return;
        }

        // Absorbed gas from surrounding area
        var gasEfficiency = GetGasEfficiency(ent.AsNullable());
        var moles = mix.TotalMoles * gasEfficiency;

        var totalDamage = 0f;

        var tempThreshold = Atmospherics.T0C + Config.GetCVar(ImpCCVars.SupermatterHeatPenaltyThreshold);

        // Temperature start to have a positive effect on damage after 350
        var tempDamage = Math.Max(Math.Clamp(moles / 200f, .5f, 1f) * mix.Temperature - tempThreshold * ent.Comp.DynamicHeatResistance, 0f) *
            ent.Comp.MoleHeatPenaltyThreshold / 150f * ent.Comp.DamageMultiplier;
        totalDamage += tempDamage;

        // Power only starts affecting damage when it is above 5000
        var powerDamage = Math.Max(ent.Comp.Power - Config.GetCVar(ImpCCVars.SupermatterPowerPenaltyThreshold), 0f) / 500f * ent.Comp.DamageMultiplier;
        totalDamage += powerDamage;

        // Mol count only starts affecting damage when it is above 1800
        var moleDamage = Math.Max(moles - Config.GetCVar(ImpCCVars.SupermatterMolePenaltyThreshold), 0f) / 80 * ent.Comp.DamageMultiplier;
        totalDamage += moleDamage;

        // Healing damage
        if (moles < Config.GetCVar(ImpCCVars.SupermatterMolePenaltyThreshold))
        {
            // Only has a net positive effect when the temp is below 313.15, heals up to 2 damage.
            // Psychologists increase this temp min by up to 45 
            var soothingValue = 0f;

            if(PsyReceiversQuery.TryComp(ent, out var psyReceiver))
                soothingValue = psyReceiver.SoothedCurrent;

            ent.Comp.HeatHealing = Math.Min(mix.Temperature - (tempThreshold + 45f * soothingValue), 0f) / 150f;
            totalDamage += ent.Comp.HeatHealing;
        }
        else
            ent.Comp.HeatHealing = 0f;

        DirtyField(ent, ent.Comp, nameof(SupermatterComponent.HeatHealing));

        // Check for space tiles next to SM
        if (TryComp<MapGridComponent>(gridId, out var grid))
        {
            var localpos = xform.Coordinates.Position;
            var tilerefs = Map.GetLocalTilesIntersecting(
                gridId.Value,
                grid,
                new Box2(localpos + new Vector2(-1, -1), localpos + new Vector2(1, 1)));

            // We should have 9 tiles in total, any less means there's a space tile nearby
            if (tilerefs.Count() < 9)
            {
                var factor = GetIntegrity(ent.AsNullable()) switch
                {
                    < 10 => 0.0005f,
                    < 25 => 0.0009f,
                    < 45 => 0.005f,
                    < 75 => 0.002f,
                    _ => 0f
                };

                totalDamage += Math.Clamp(ent.Comp.Power * factor * ent.Comp.DamageMultiplier, 0, ent.Comp.MaxSpaceExposureDamage);
            }
        }

        var damage = Math.Min(ent.Comp.DamageArchived + ent.Comp.MaximumDamagePerCycle * ent.Comp.DamageDelaminationThreshold, ent.Comp.Damage + totalDamage);

        // Prevent it from going negative
        ent.Comp.Damage = Math.Clamp(damage, 0, float.PositiveInfinity);

        var actualDamage = ent.Comp.Damage - ent.Comp.DamageArchived;

        // Check if the actual damage is close to zero
        if(MathHelper.CloseTo(actualDamage, 0f, float.Epsilon))
            return;

        var damageEv = new SupermatterDamagedEvent();
        RaiseLocalEvent(ent, ref damageEv);
    }
}
