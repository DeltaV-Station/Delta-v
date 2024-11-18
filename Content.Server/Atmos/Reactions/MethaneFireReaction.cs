using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Explosion;
using Robust.Shared.Prototypes;
using Content.Server.EUI;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Content.Shared.Maps;
using Content.Shared.Coordinates;

namespace Content.Server.Atmos.Reactions
{
    [UsedImplicitly]
    [DataDefinition]
    public sealed partial class MethaneFireReaction : IGasReactionEffect
    {
        [Dependency] private readonly IEntitySystemManager _entitySystem = default!;
        //[Dependency] private readonly ExplosionSystem _explosionSystem = default!;
        //[Dependency] private readonly SharedTransformSystem _transform = default!;
        public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
        {
            var energyReleased = 0f;
            var oldHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);
            var temperature = mixture.Temperature;
            var location = holder as TileAtmosphere;
            mixture.ReactionResults[GasReaction.Fire] = 0;

            // More Methane released at higher temperatures.
            var temperatureScale = 0f;

            if (temperature > Atmospherics.MethaneUpperTemperature)
                temperatureScale = 1f;
            else
            {
                temperatureScale = (temperature - Atmospherics.MethaneMinimumBurnTemperature) /
                                   (Atmospherics.MethaneUpperTemperature - Atmospherics.MethaneMinimumBurnTemperature);
            }

            if (temperatureScale > 0)
            {
                var oxygenBurnRate = Atmospherics.OxygenBurnRateBase - temperatureScale;
                var MethaneBurnRate = 0f;

                var initialOxygenMoles = mixture.GetMoles(Gas.Oxygen);
                var initialMethaneMoles = mixture.GetMoles(Gas.Methane);

                // Supersaturation makes tritium.
                var oxyRatio = initialOxygenMoles / initialMethaneMoles;
                // Efficiency of reaction decreases from 1% Methane to 3% Methane:

                if (initialOxygenMoles > initialMethaneMoles * Atmospherics.MethaneOxygenFullburn)
                    MethaneBurnRate = initialMethaneMoles * temperatureScale / Atmospherics.MethaneBurnRateDelta;
                else
                    MethaneBurnRate = temperatureScale * (initialOxygenMoles / Atmospherics.MethaneOxygenFullburn) / Atmospherics.MethaneBurnRateDelta;

                if (MethaneBurnRate > Atmospherics.MinimumHeatCapacity)
                {
                    MethaneBurnRate = MathF.Min(MethaneBurnRate, MathF.Min(initialMethaneMoles, initialOxygenMoles / oxygenBurnRate));
                    mixture.SetMoles(Gas.Methane, MathF.Max(0,initialMethaneMoles - MethaneBurnRate));
                    mixture.SetMoles(Gas.Oxygen, MathF.Max(0,initialOxygenMoles - MethaneBurnRate * oxygenBurnRate));

                    mixture.AdjustMoles(Gas.CarbonDioxide, MethaneBurnRate);

                    //if(mixture.GetMoles(Gas.Methane)>100&&mixture.GetMoles(Gas.Oxygen)>10){
                    //    initialOxygenMoles = mixture.GetMoles(Gas.Oxygen);
                    //    initialMethaneMoles = mixture.GetMoles(Gas.Methane);
                    //    IoCManager.InjectDependencies(this);
                    //    if (location != null){
                    //        TileRef aaa = atmosphereSystem.GetTileRef(location);
                    //        TransformComponent? tc = IoCManager.Resolve<IEntityManager>().GetComponentOrNull<TransformComponent>(aaa.GridUid);
                    //        if(tc!=null){
                    //            _entitySystem.GetEntitySystem<ExplosionSystem>().QueueExplosion(new MapCoordinates(aaa.X,aaa.Y,tc.MapID), ExplosionSystem.DefaultExplosionPrototypeId, (initialMethaneMoles-100)/150, 0.1f, (initialMethaneMoles-100)/300);
                    //        }
                    //        //_entitySystem.GetEntitySystem<ExplosionSystem>().QueueExplosion(location.GridIndex, ExplosionSystem.DefaultExplosionPrototypeId, (initialMethaneMoles-100)/150, 0.1f, (initialMethaneMoles-100)/300);
                    //    }
                    //    initialOxygenMoles = mixture.GetMoles(Gas.Oxygen);
                    //    initialMethaneMoles = mixture.GetMoles(Gas.Methane);
                    //    mixture.SetMoles(Gas.Methane, MathF.Max(0,initialMethaneMoles - MethaneBurnRate*25));
                    //    mixture.SetMoles(Gas.Oxygen, MathF.Max(0,initialOxygenMoles - MethaneBurnRate * oxygenBurnRate*25));
                    //    mixture.AdjustMoles(Gas.CarbonDioxide, MethaneBurnRate*25);
                    //    energyReleased += Atmospherics.FireMethaneEnergyReleased * MethaneBurnRate*5;
                    //    energyReleased /= heatScale; // adjust energy to make sure speedup doesn't cause mega temperature rise
                    //    mixture.ReactionResults[GasReaction.Fire] += MethaneBurnRate * (1 + oxygenBurnRate)*5;
//
//
                    //}else{

                        energyReleased += Atmospherics.FireMethaneEnergyReleased * MethaneBurnRate;
                        energyReleased /= heatScale*5; // adjust energy to make sure speedup doesn't cause mega temperature rise
                        mixture.ReactionResults[GasReaction.Fire] += MethaneBurnRate * (1 + oxygenBurnRate);
                    //}
                }
            }

            if (energyReleased > 0)
            {
                var newHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);
                if (newHeatCapacity > Atmospherics.MinimumHeatCapacity)
                    mixture.Temperature = (temperature * oldHeatCapacity + energyReleased) / newHeatCapacity;
            }

            if (location != null)
            {
                var mixTemperature = mixture.Temperature;
                if (mixTemperature > Atmospherics.FireMinimumTemperatureToExist)
                {
                    atmosphereSystem.HotspotExpose(location, mixTemperature, mixture.Volume);
                }
            }

            return mixture.ReactionResults[GasReaction.Fire] != 0 ? ReactionResult.Reacting : ReactionResult.NoReaction;
        }
    }
}
