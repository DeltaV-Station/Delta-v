using Content.Server._DV.CosmicCult.EntitySystems;
using Content.Shared.Maps;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._DV.CosmicCult.Components;

[RegisterComponent, Access(typeof(CosmicCorruptingSystem))]
[AutoGenerateComponentPause]
public sealed class CosmicCorruptingComponent : Component
{
    /// <summary>
    /// Wether or not the CosmicCorruptingSystem should ignore this component when it reaches max growth. Saves performance.
    /// </summary>
    [DataField]
    public bool AutoDisable = true;

    /// <summary>
    /// the door we spawn when replacing a secret door
    /// </summary>
    [DataField]
    public EntProtoId ConversionDoor = "DoorCosmicCult";

    /// <summary>
    /// The tile we spawn when replacing a normal tile.
    /// </summary>
    [DataField] //not a dict like the entity conversion below because there's too many fucking tiles
    public ProtoId<ContentTileDefinition> ConversionTile = "FloorCosmicCorruption";

    /// <summary>
    /// the list of tiles that can be corrupted by this corruptor.
    /// </summary>
    [DataField]
    public HashSet<Vector2i> CorruptableTiles = [];

    /// <summary>
    /// The chance that a tile and/or wall is replaced.
    /// </summary>
    [DataField]
    public float CorruptionChance = 0.51f;

    /// <summary>
    /// The maximum amount of ticks this source can do.
    /// </summary>
    [DataField]
    public int CorruptionMaxTicks = 50;

    /// <summary>
    /// The reduction applied to corruption chance every tick.
    /// </summary>
    [DataField]
    public float CorruptionReduction;

    /// <summary>
    /// How much time between tile corruptions.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan CorruptionSpeed = TimeSpan.FromSeconds(6);

    /// <summary>
    /// How many times has this corruption source ticked?
    /// </summary>
    [DataField]
    public int CorruptionTicks;

    /// <summary>
    /// Our timer for corruption checks.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField] public TimeSpan CorruptionTimer;

    /// <summary>
    /// Wether or not the CosmicCorruptingSystem should be running on this entity. use CosmicCorruptingSystem.Enable() instead of directly interacting with this variable.
    /// </summary>
    [DataField]
    public bool Enabled = true;

    /// <remarks>
    /// this data entry brought to you by UNKILLABLE ANGEL - Ada Rook
    /// </remarks>
    /// <summary>
    /// The dict that we look through to determine what entities should be converted and what they should be converted into.
    /// absolutely fucking massive so that multiple things don't need to re-specify it in yamlland
    /// </summary>
    [DataField]
    public Dictionary<EntProtoId, EntProtoId> EntityConversionDict = new()
    {
        //walls
        {"WallSolid", "WallCosmicCult"},
        {"WallSolidRust", "WallCosmicCult"},
        {"WallReinforced", "WallCosmicCult"},
        {"WallReinforcedRust", "WallCosmicCult"},
        {"WallShuttleInterior", "WallCosmicCult"},
        {"WallShuttle", "WallCosmicCult"},
        {"WallMining", "WallCosmicCult"},
        {"WallAndesiteCobblebrick", "WallCosmicCult"},
        {"WallAsteroidCobblebrick", "WallCosmicCult"},
        {"WallClown", "WallCosmicCult"},
        {"WallVaultAlien", "WallCosmicCult"},
        {"WallBasaltCobblebrick", "WallCosmicCult"},
        {"WallBrick", "WallCosmicCult"},
        {"WallChromiteCobblebrick", "WallCosmicCult"},
        {"WallCobblebrick", "WallCosmicCult"},
        {"WallCult", "WallCosmicCult"}, //the cooler cult wins here
        {"WallDiamond", "WallCosmicCult"},
        {"WallGold", "WallCosmicCult"},
        {"WallIce", "WallCosmicCult"},
        {"WallPlasma", "WallCosmicCult"},
        {"WallPlastic", "WallCosmicCult"},
        {"WallVaultRock", "WallCosmicCult"},
        {"WallSandCobblebrick", "WallCosmicCult"},
        {"WallVaultSandstone", "WallCosmicCult"},
        {"WallSandstone", "WallCosmicCult"},
        {"WallSilver", "WallCosmicCult"},
        {"WallSnowCobblebrick", "WallCosmicCult"},
        {"WallNecropolis", "WallCosmicCult"},
        {"WallUranium", "WallCosmicCult"},
        {"WallWood", "WallCosmicCult"},
        {"WallClock", "WallCosmicCult"},
        //ignoring meat walls & asteroid for being organic, + girders & inflatable walls for being cheap and easy to spam
        //ignoring diagonals because they're not real

        //doors
        {"SolidSecretDoor", "DoorCosmicCult"},
        //ignoring real doors because I don't want to figure out copying accesses over

        //windows
        {"Window", "WindowCosmicCult"},
        {"ReinforcedWindow", "WindowCosmicCult"},
        {"PlasmaWindow", "WindowCosmicCult"},
        {"ReinforcedPlasmaWindow", "WindowCosmicCult"},
        {"UraniumWindow", "WindowCosmicCult"},
        {"ReinforcedUraniumWindow", "WindowCosmicCult"},
        {"TintedWindow", "WindowCosmicCultDark"},
        {"ClockworkWindow", "WindowCosmicCult"},
        {"ShuttleWindow", "WindowCosmicCult"},
        {"MiningWindow", "WindowCosmicCult"},
        //ignoring diagonals because they're not real

        //furniture
        //tables
        {"Table", "CosmicTable"},
        {"TableBrass", "CosmicTable"},
        {"TableFancyBlack", "CosmicTable"},
        {"TableFancyBlue", "CosmicTable"},
        {"TableFancyCyan", "CosmicTable"},
        {"TableFancyGreen", "CosmicTable"},
        {"TableFancyOrange", "CosmicTable"},
        {"TableFancyPink", "CosmicTable"},
        {"TableFancyPurple", "CosmicTable"},
        {"TableFancyRed", "CosmicTable"},
        {"TableFancyWhite", "CosmicTable"},
        {"TableCarpet", "CosmicTable"},
        {"TableGlass", "CosmicTable"},
        {"TableReinforcedGlass", "CosmicTable"},
        {"TableCounterMetal", "CosmicTable"},
        {"TableReinforced", "CosmicTable"},
        {"TableWood", "CosmicTable"},
        {"TableCounterWood", "CosmicTable"},
        {"TableWoodReinforced", "CosmicTable"},
        {"TableStone", "CosmicTable"},
        //chairs
        {"Chair", "CosmicChair"},
        {"ChairGreyscale", "CosmicChair"},
        {"ComfyChair", "CosmicChair"},
        {"ChairPilotSeat", "CosmicChair"},
        {"ChairBrass", "CosmicChair"},
        //ignoring office chairs and a few others because they don't need to be anchored
        //if I missed something yell at me
    };

    /// <summary>
    /// if this corruption source should floodfill through all corruptible tiles to initialise its corruptible tile set on activation.
    /// </summary>
    [DataField]
    public bool FloodFillStarting;

    /// <summary>
    /// If this corruption source can move. if true, only corrupt the immediate area around it.
    /// Slightly hacky but works for our purposes.
    /// </summary>
    [DataField]
    public bool Mobile;

    /// <summary>
    /// The VFX entity we spawn when corruption occurs.
    /// </summary>
    [DataField]
    public EntProtoId TileConvertVFX = "CosmicFloorSpawnVFX";

    /// <summary>
    /// The VFX entity we spawn when walls get deleted.
    /// </summary>
    [DataField]
    public EntProtoId TileDisintegrateVFX = "CosmicGenericVFX";

    /// <summary>
    /// Wether or not the CosmicCorruptingSystem should spawn VFX when converting tiles and walls.
    /// </summary>
    [DataField]
    public bool UseVFX = true;
}
