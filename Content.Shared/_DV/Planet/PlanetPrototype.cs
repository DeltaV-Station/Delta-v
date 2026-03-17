using Content.Shared.Atmos;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Parallax.Biomes.Markers;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._DV.Planet;

[Prototype]
public sealed partial class PlanetPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// The biome to create the planet with.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<BiomeTemplatePrototype> Biome;

    /// <summary>
    /// Name to give to the map.
    /// </summary>
    [DataField(required: true)]
    public LocId MapName;

    /// <summary>
    /// Ambient lighting for the map.
    /// </summary>
    [DataField]
    public Color MapLight = Color.FromHex("#D8B059");

    /// <summary>
    /// Components to add to the map.
    /// </summary>
    [DataField]
    public ComponentRegistry? AddedComponents;

    /// <summary>
    /// The gas mixture to use for the atmosphere.
    /// </summary>
    [DataField(required: true)]
    public GasMixture Atmosphere = new();

    /// <summary>
    /// Biome layers to add to the map, i.e. ores.
    /// </summary>
    [DataField]
    public List<ProtoId<BiomeMarkerLayerPrototype>> BiomeMarkerLayers = new();

    /// <summary>
    /// Ruin map paths for the ruin pool.
    /// </summary>
    [DataField]
    public List<ResPath> RuinPaths = new();

    /// <summary>
    /// Minimum number of ruins to spawn at round start.
    /// </summary>
    [DataField]
    public int RuinMinCount = 0;

    /// <summary>
    /// Maximum number of ruins to spawn at round start.
    /// </summary>
    [DataField]
    public int RuinMaxCount = 0;

    /// <summary>
    /// Optional rare ruin map paths that should be selected less often.
    /// </summary>
    [DataField]
    public List<ResPath> RareRuinPaths = new();

    /// <summary>
    /// Number of rare ruins guaranteed.
    /// </summary>
    [DataField]
    public int GuaranteedRareRuins = 0;

    /// <summary>
    /// Percent chance that each additional ruin selected is rare.
    /// </summary>
    [DataField]
    public int RuinRareChance = 0;
}
