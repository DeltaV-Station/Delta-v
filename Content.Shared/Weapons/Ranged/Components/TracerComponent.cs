using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Ranged.Components;

/// <summary>
/// Added to projectiles to give them tracer effects
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TracerComponent : Component
{
    /// <summary>
    /// How long the tracer effect should remain visible for after firing
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Lifetime = 10f;

    /// <summary>
    /// The maximum length of the tracer trail
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Length = 2f;

    /// <summary>
    /// Color of the tracer line effect
    /// </summary>
    [DataField, AutoNetworkedField]
    public Color Color = Color.Red;
}
