namespace Content.Shared._DV.MetalDetector;

/// <summary>
/// Responsible for holding some data of whom has passed by and which item is the most dangerous.
/// </summary>
[RegisterComponent]
public sealed partial class MetalDetectorComponent : Component
{
    [DataField]
    public EntityUid LastPassingPlayer;
}
