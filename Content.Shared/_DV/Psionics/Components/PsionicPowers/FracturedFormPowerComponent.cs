using Content.Shared.Cloning;
using Content.Shared.Roles;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class FracturedFormPowerComponent : BasePsionicPowerComponent
{
    public override EntProtoId ActionProtoId { get; set; } = "ActionFracturedForm";

    public override string PowerName { get; set; } = "psionic-power-name-fractured-form";

    public override int MinGlimmerChanged { get; set; } = 5;

    public override int MaxGlimmerChanged { get; set; } = 30;

    /// <summary>
    /// The chance for the clone to have the starting gear of the psionic.
    /// </summary>
    [DataField]
    public float HasGearChance = 0.4f;

    /// <summary>
    /// The chance for the clone to have a different species than the psionic.
    /// </summary>
    [DataField]
    public float DifferentSpeciesChance = 0.6f;

    /// <summary>
    /// These are settings for creating the second body.
    /// </summary>
    [DataField]
    public ProtoId<CloningSettingsPrototype> CopyNaked = "CloningPod";

    /// <summary>
    /// These are settings for creating the second body.
    /// </summary>
    [DataField]
    public ProtoId<CloningSettingsPrototype> CopyClothed = "Antag";

    /// <summary>
    /// These are settings for creating the second body.
    /// </summary>
    [DataField]
    public ProtoId<JobPrototype> VisitorJob = "Passenger";

    /// <summary>
    /// These are settings for creating the second body.
    /// </summary>
    [DataField]
    public ProtoId<JobPrototype>? NakedJob; // Scary null, but we explicitly want no job for naked spawns.

    /// <summary>
    /// The minimum time between forced swaps.
    /// </summary>
    [DataField]
    public TimeSpan NextSwapMinTime = TimeSpan.FromMinutes(5);

    /// <summary>
    /// The maximum time between swaps.
    /// </summary>
    [DataField]
    public TimeSpan NextSwapMaxTime = TimeSpan.FromMinutes(20);

    /// <summary>
    /// The current timer for when the next swap is forced.
    /// </summary>
    [DataField, AutoPausedField]
    public TimeSpan NextSwap = TimeSpan.FromMinutes(5);

    /// <summary>
    /// The minimum time between voluntary swaps by sleeping.
    /// </summary>
    [DataField]
    public TimeSpan VoluntarySwapCooldown = TimeSpan.FromSeconds(10);

    /// <summary>
    /// The minimum time between voluntary swaps by sleeping.
    /// </summary>
    [DataField, AutoPausedField]
    public TimeSpan NextVoluntarySwap = TimeSpan.FromSeconds(10);

    /// <summary>
    /// The length of the DoAfter for swapping bodies.
    /// </summary>
    [DataField]
    public float ManualSwapTime = 5f;

    /// <summary>
    /// The time the psionic needs to be asleep to swap bodies.
    /// </summary>
    [DataField]
    public TimeSpan SwapTimeAsleep = TimeSpan.FromSeconds(3);

    /// <summary>
    /// The time length where the warning is sent before the forced swap.
    /// </summary>
    [DataField]
    public TimeSpan WarningTimeBeforeSleep = TimeSpan.FromSeconds(5);

    /// <summary>
    /// A boolean check for when the person has been warned.
    /// </summary>
    [DataField]
    public bool SleepWarned;

    /// <summary>
    /// The bodies that the psionic user can swap into.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<EntityUid> Bodies { get; set; } = [];

    /// <summary>
    /// The sound that plays when the swap happens.
    /// </summary>
    [DataField]
    public SoundSpecifier SwapSound = new SoundPathSpecifier("/Audio/Effects/teleport_arrival.ogg")
    {
        Params = AudioParams.Default.WithVariation(0.05f),
    };
}
