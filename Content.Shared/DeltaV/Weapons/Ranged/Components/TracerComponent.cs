using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Shared.DeltaV.Weapons.Ranged.Components;

/// <summary>
/// Added to projectiles to give them tracer effects
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TracerComponent : Component
{
    /// <summary>
    /// How long the tracer effect should remain visible for after firing
    /// </summary>
    [DataField]
    public float Lifetime = 10f;

    /// <summary>
    /// The maximum length of the tracer trail
    /// </summary>
    [DataField]
    public float Length = 2f;

    /// <summary>
    /// Color of the tracer line effect
    /// </summary>
    [DataField]
    public Color Color = Color.Red;

    public TracerData Data = default!;
}

public sealed class TracerData
{
    public List<Vector2> PositionHistory = new();
    public TimeSpan EndTime;
}
