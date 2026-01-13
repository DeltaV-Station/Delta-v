using Robust.Shared.GameStates;
namespace Content.Shared._DV.Vision.Components;

/// <summary>
/// This is used for providing psychological soothing to receivers.
/// </summary>
[RegisterComponent][NetworkedComponent]
public sealed partial class PsychologicalSoothingProviderComponent : Component
{
    /// <summary>
    /// The radius in which to provide soothing.
    /// </summary>
    [DataField]
    public float Range = 20f;

    /// <summary>
    /// The <see cref="PsychologicalSoothingReceiverComponent.RateGrowth"/> is multiplied by this value.
    /// </summary>
    [DataField]
    public float Strength = 1f;
}
