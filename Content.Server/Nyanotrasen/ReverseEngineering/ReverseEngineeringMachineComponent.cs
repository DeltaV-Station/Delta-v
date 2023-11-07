using Content.Shared.ReverseEngineering;
using Content.Shared.Construction.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Audio;

namespace Content.Server.ReverseEngineering;

/// <summary>
/// This machine can reverse engineer items and get a technology disk from them.
/// </summary>
[RegisterComponent]
public sealed partial class ReverseEngineeringMachineComponent : Component
{
    [DataField("diskPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string DiskPrototype = "TechnologyDisk";

    [DataField("machinePartScanBonus", customTypeSerializer: typeof(PrototypeIdSerializer<MachinePartPrototype>))]
    public string MachinePartScanBonus = "MatterBin"; // DeltaV Code: Change part checked for bonus to MatterBin as it is what is used in the crafting recipe

    /// <summary>
    /// Added to the 3d6, scales off of scanner.
    /// </summary>
    public int ScanBonus = 1;


    [DataField("machinePartDangerAversionScore", customTypeSerializer: typeof(PrototypeIdSerializer<MachinePartPrototype>))]
    public string MachinePartDangerAversionScore = "Manipulator";

    /// <summary>
    /// If we rolled destruction, this is added to the roll and if it <= 9 it becomes
    /// stagnation instead.
    /// </summary>
    public int DangerAversionScore = 1;

    /// <summary>
    /// Whether the machine is going to receive the danger bonus.
    /// </summary>
    [DataField("dangerBonus")]
    public int DangerBonus = 3;

    [DataField("failSound1")]
    public SoundSpecifier FailSound1 { get; set; } = new SoundPathSpecifier("/Audio/Effects/spray.ogg");

    [DataField("failSound2")]
    public SoundSpecifier FailSound2 { get; set; } = new SoundPathSpecifier("/Audio/Effects/sparks4.ogg");

    [DataField("successSound")]
    public SoundSpecifier SuccessSound { get; set; } = new SoundPathSpecifier("/Audio/Machines/microwave_done_beep.ogg");

    [DataField("clickSound")]
    public SoundSpecifier ClickSound = new SoundPathSpecifier("/Audio/Machines/machine_switch.ogg");
    public EntityUid? CurrentItem;

    /// <summary>
    /// Malus from the item's difficulty.
    /// </summary>
    [ViewVariables]
    public int CurrentItemDifficulty = 0;

    /// <summary>
    /// Whether the safety is on.
    /// </summary>
    public bool SafetyOn = true;

    /// <summary>
    /// Whether autoscan is on.
    /// </summary>
    public bool AutoScan = true;

    public int Progress = 0;

    public TimeSpan AnalysisDuration = TimeSpan.FromSeconds(30);

    public FormattedMessage? CachedMessage;

    public ReverseEngineeringTickResult? LastResult;
}
