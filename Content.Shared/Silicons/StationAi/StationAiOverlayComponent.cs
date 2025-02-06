using Robust.Shared.GameStates;

namespace Content.Shared.Silicons.StationAi;

/// <summary>
/// Handles the static overlay for station AI.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState] // DeltaV
public sealed partial class StationAiOverlayComponent : Component
{
    /// <summary>
    /// DeltaV: Makes this purely an overlay and not functional.
    /// Exists because this component also controls interaction ranges for some reason.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Cosmetic;
}
