using Content.Shared.Construction.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.ReverseEngineering;

/// <summary>
/// This machine can reverse engineer items and get a technology disk from them.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedReverseEngineeringSystem))]
[AutoGenerateComponentState]
public sealed partial class ReverseEngineeringMachineComponent : Component
{
    /// <summary>
    /// Name of the slot in <c>ItemSlotsComponent</c> that stores the target item.
    /// </summary>
    [DataField]
    public string Slot = "target_slot";

    /// <summary>
    /// Tech disk prototype to spawn after completion.
    /// Must have <c>TechDiskComponent</c>
    /// </summary>
    [DataField]
    public EntProtoId DiskPrototype = "TechnologyDisk";

    /// <summary>
    /// Added to the 3d6, scales off of scanner.
    /// </summary>
    [DataField]
    public int ScanBonus = 1;

    /// <summary>
    /// If we rolled destruction, this is added to the roll and if it <= 9 it becomes
    /// stagnation instead.
    /// </summary>
    [DataField]
    public int DangerAversionScore = 1;

    /// <summary>
    /// Whether the machine is going to receive the danger bonus.
    /// </summary>
    [DataField]
    public int DangerBonus = 3;

    /// <summary>
    /// Sounds simultaneously played when an item is destroyed.
    /// </summary>
    [DataField]
    public List<SoundSpecifier> FailSounds = new()
    {
        new SoundPathSpecifier("/Audio/Effects/spray.ogg"),
        new SoundPathSpecifier("/Audio/Effects/sparks4.ogg")
    };

    /// <summary>
    /// Sound played when an item is successfully reverse engineered.
    /// </summary>
    [DataField]
    public SoundSpecifier? SuccessSound = new SoundPathSpecifier("/Audio/Machines/microwave_done_beep.ogg");

    [DataField]
    public SoundSpecifier? ClickSound = new SoundPathSpecifier("/Audio/Machines/machine_switch.ogg");

    /// <summary>
    /// Whether the safety is on.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool SafetyOn = true;

    /// <summary>
    /// Whether autoscan is on.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool AutoScan = true;

    /// <summary>
    /// How long to wait between analysis rolls.
    /// </summary>
    [DataField]
    public TimeSpan AnalysisDuration = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Last result to show in the ui
    /// </summary>
    [DataField, AutoNetworkedField]
    public ReverseEngineeringTickResult? LastResult;
}
