using Content.Shared._EE.FootPrint.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared._EE.FootPrint;

/// <summary>
/// Component for puddles that can leave footprints when stepped on.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(PuddleFootPrintsSystem))]
public sealed partial class PuddleFootPrintsComponent : Component
{
    /// <summary>
    /// The ratio of the puddle's volume that determines color intensity transferred to footprints.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float SizeRatio = 0.2f;

    /// <summary>
    /// Minimum percentage of water content required before the puddle will transfer footprints.
    /// Prevents pure water puddles from leaving colored footprints.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float OffPercent = 80f;
}
