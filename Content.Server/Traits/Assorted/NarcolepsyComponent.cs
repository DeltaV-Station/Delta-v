using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom; // DeltaV Narcolepsy
using System.Numerics;
using Content.Shared.Dataset;
using Robust.Shared.Prototypes;

namespace Content.Server.Traits.Assorted;

/// <summary>
/// This is used for the narcolepsy trait.
/// </summary>
[RegisterComponent, Access(typeof(NarcolepsySystem))]
public sealed partial class NarcolepsyComponent : Component
{
    /// <summary>
    /// The random time between incidents, (min, max).
    /// </summary>
    [DataField("timeBetweenIncidents", required: true)]
    public Vector2 TimeBetweenIncidents { get; private set; }

    /// <summary>
    /// The duration of incidents, (min, max).
    /// </summary>
    [DataField("durationOfIncident", required: true)]
    public Vector2 DurationOfIncident { get; private set; }

    [DataField] // # DeltaV begin Narcolepsy port from EE
    public float  NextIncidentTime;

    /// <summary>
    /// Dataset to pick warning strings from.
    /// </summary>
    [DataField]
    public ProtoId<LocalizedDatasetPrototype> NarcolepsyWarningDataset = "NarcolepsyWarning";

    /// <summary>
    /// Dataset to pick wakeup strings from.
    /// </summary>
    [DataField]
    public ProtoId<LocalizedDatasetPrototype> NarcolepsyWakeupDataset = "NarcolepsyWakeup";

    [DataField]
    public int WarningLocaleCount = 5, WakeupLocaleCount = 3;

    [DataField]
    public float TimeBeforeWarning = 20f, WarningChancePerSecond = 0.25f;

    public float LastWarningRollTime = float.MaxValue; // DeltaV end Narcolepsy port from EE
}
