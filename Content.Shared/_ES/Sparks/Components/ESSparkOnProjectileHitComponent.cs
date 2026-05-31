using Content.Shared.Projectiles;
using Robust.Shared.GameStates;

namespace Content.Shared._ES.Sparks.Components;

/// <summary>
/// An entity with <see cref="ProjectileComponent"/> that sparks when hitting something
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(ESSparksSystem))]
public sealed partial class ESSparkOnProjectileHitComponent : ESBaseSparkConfigurationComponent;
