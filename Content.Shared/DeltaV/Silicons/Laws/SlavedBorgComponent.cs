using Content.Shared.Silicons.Laws;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeltaV.Silicons.Laws;

/// <summary>
/// Adds a law no matter the default lawset.
/// Switching borg chassis type keeps this law.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedSlavedBorgSystem))]
public sealed partial class SlavedBorgComponent : Component
{
    /// <summary>
    /// The law to add after loading the default laws or switching chassis.
    /// This is assumed to be law 0 so gets inserted to the top of the laws.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<SiliconLawPrototype> Law;

    /// <summary>
    /// Prevents adding the same law twice.
    /// </summary>
    [DataField]
    public bool Added;
}
