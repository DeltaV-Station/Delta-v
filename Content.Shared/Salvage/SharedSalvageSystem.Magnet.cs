using Content.Shared.Destructible.Thresholds;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Procedural;
using Content.Shared.Procedural.DungeonLayers;
using Content.Shared.Procedural.PostGeneration;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Content.Shared.Salvage.Magnet;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Shared.Salvage;

public abstract partial class SharedSalvageSystem
{
    //private readonly List<SalvageMapPrototype> _salvageMaps = new(); DeltaV - Salvage wreck maps are no longer used

    private readonly Dictionary<ISalvageMagnetOffering, float> _offeringWeights = new()
    {
        { new AsteroidOffering(), 3f }, // DeltaV: was 4.5f
        { new DebrisOffering(), 1.5f }, // DeltaV: was 3.5f
        // { new SalvageOffering(), 1.5f } DeltaV: SalvageOffering is removed
    };

    //BEGIN DeltaV - weight asteroid generation (to balance the varying Wreck spawns they each support)
    private readonly Dictionary<ProtoId<DungeonConfigPrototype>, float> _asteroidConfigs = new()
    {
        { "GiantBlobAsteroid", 0.25f},
        { "BlobAsteroid", 1f},
        { "ClusterAsteroid", 1.3f},
        { "SpindlyAsteroid", 1.3f},
        { "SwissCheeseAsteroid", 1.3f}
    };
    //DeltaV end

    private readonly ProtoId<WeightedRandomPrototype> _asteroidOreWeights = "AsteroidOre";

    private readonly MinMax _asteroidOreCount = new(2, 4);

    private readonly List<ProtoId<DungeonConfigPrototype>> _debrisConfigs = new()
    {
        "ChunkDebris"
    };

    private readonly List<ProtoId<BiomeTemplatePrototype>> _debrisLootConfigs = new()
    {
        "SpaceDebrisLootRegular",
        "SpaceDebrisLootScrap",
        "SpaceDebrisLootValuables",
        "SpaceDebrisLootArcana"
    };

    public ISalvageMagnetOffering GetSalvageOffering(int seed)
    {
        var rand = new System.Random(seed);

        var type = SharedRandomExtensions.Pick(_offeringWeights, rand);
        switch (type)
        {
            case AsteroidOffering:
                //var configId = _asteroidConfigs[rand.Next(_asteroidConfigs.Count)]; // DeltaV - we add weights to the asteroid types
                var configId = SharedRandomExtensions.Pick(_asteroidConfigs, rand); //DeltaV - we add weights to the asteroid types
                var configProto = _proto.Index(configId);
                var layers = new Dictionary<string, int>();

                var config = new DungeonConfig
                {
                    Layers = new(configProto.Layers),
                    MaxCount = configProto.MaxCount,
                    MaxOffset = configProto.MaxOffset,
                    MinCount = configProto.MinCount,
                    MinOffset = configProto.MinOffset,
                    ReserveTiles = configProto.ReserveTiles
                };

                var count = _asteroidOreCount.Next(rand);
                var weightedProto = _proto.Index(_asteroidOreWeights);
                for (var i = 0; i < count; i++)
                {
                    var ore = weightedProto.Pick(rand);
                    config.Layers.Add(_proto.Index<OreDunGenPrototype>(ore));

                    var layerCount = layers.GetOrNew(ore);
                    layerCount++;
                    layers[ore] = layerCount;
                }

                return new AsteroidOffering
                {
                    Id = configId,
                    DungeonConfig = config,
                    MarkerLayers = layers,
                };
            case DebrisOffering:
                var id = rand.Pick(_debrisConfigs);
                //BEGIN DeltaV - Debris generation breaks loot out as a separatly added layer to support dynamic generation. Mirrors asteroids vs ore.
                var debrisConfigProto = _proto.Index(id);

                var debrisConfig = new DungeonConfig
                {
                    Layers = new(debrisConfigProto.Layers),
                    MaxCount = debrisConfigProto.MaxCount,
                    MaxOffset = debrisConfigProto.MaxOffset,
                    MinCount = debrisConfigProto.MinCount,
                    MinOffset = debrisConfigProto.MinOffset,
                    ReserveTiles = debrisConfigProto.ReserveTiles
                };

                var debrisLootConfig = _debrisLootConfigs[rand.Next(_debrisLootConfigs.Count)];

                debrisConfig.Layers.Add(new BiomeDunGen()
                {
                    BiomeTemplate = _proto.Index(debrisLootConfig)
                });

                return new DebrisOffering
                {
                    Id = id,
                    DungeonConfig = debrisConfig,
                    LootId = debrisLootConfig.Id
                };
                //END DeltaV

            default:
                throw new NotImplementedException($"Salvage type {type} not implemented!");
        }
    }
}
