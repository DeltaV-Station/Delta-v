using Content.Shared.Atmos;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Projectiles;

/// <summary>
/// Modifies projectile damage when atmospheric pressure is above a threshold.
/// Default is for a PKA bolt.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedPressureProjectileSystem))]
public sealed partial class PressureProjectileComponent : Component
{
    /// <summary>
    /// Max pressure to allow full damage at.
    /// If it exceeds this at point of impact, damage gets modified by <see cref="Modifier"/>.
    /// </summary>
    [DataField]
    public float MaxPressure = Atmospherics.OneAtmosphere * 0.5f;

    /// <summary>
    /// Multiplies projectile damage by this modifier when below <see cref="MaxPressure"/>.
    /// </summary>
    [DataField]
    public float Modifier = 0.25f;
}
