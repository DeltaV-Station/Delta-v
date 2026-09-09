using Robust.Shared.GameStates;
using Robust.Shared.Timing;

namespace Content.Shared._DV.ShadowWalk;

/// <summary>
/// Lets this entity walk straight through solid static objects (walls, doors, windows...)
/// while the entity itself is bathed in darkness (the same light level it heals in.)
/// Mobs and projectiles always stay solid.
/// <para>
/// On collision, checks our light level. Objects we're stuck in are tagged in <see cref="PassableEntities"/>
/// </para>
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowWalkerComponent : Component
{
    /// <summary>
    /// Light level below which an object counts as bathed in darkness.
    /// If the entity has a <c>LightLevelHealthComponent</c> its DarkThreshold is used
    ///   instead, so objects are passable exactly where the entity would heal.
    /// </summary>
    [DataField]
    public float DarkThreshold = 0.3f;

    /// <summary>
    /// Objects we're currently in. Objects in this list are never solid until we fully leave.
    /// </summary>
    public HashSet<EntityUid> PassableEntities = new();

    /// <summary>
    /// Light level for this tick, to avoid re-calculating for more than one collision a tick.
    /// </summary>
    public GameTick LastLightCheckTick = GameTick.Zero;

    public float LastLightLevel;
}
