using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.ShadowWalk;

/// <summary>
/// Lets this entity walk straight through solid static objects (walls, doors, windows...)
/// that are bathed in darkness.
/// <para>
/// The server periodically measures the light level at nearby static blockers and networks
/// the set of currently-passable ones in <see cref="PassableEntities"/>. Collision filtering
/// on both sides only ever consults that replicated set, never client-side lighting, so
/// prediction always agrees with the server even though off-screen lights don't exist on the client.
/// </para>
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
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
    /// Radius around the walker that is scanned for static blockers.
    /// Needs to comfortably cover the distance the walker can move between two scans, plus the client's prediction window.
    /// </summary>
    [DataField]
    public float Range = 3f;

    /// <summary>
    /// How often the passable set is rescanned.
    /// </summary>
    [DataField]
    public TimeSpan UpdateFrequency = TimeSpan.FromSeconds(0.5);

    /// <summary>
    /// The next time the passable set gets rescanned.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    /// <summary>
    /// Server-computed set of static entities this walker currently phases through. Never written on the client; both sides filter collisions purely from this set.
    /// </summary>
    [AutoNetworkedField]
    public HashSet<EntityUid> PassableEntities = new();

    /// <summary>
    /// Client-only copy of the last passable set we rebuilt contacts for. Component states are
    /// re-applied every prediction reset, so this is used to only regenerate contacts when the set actually changed.
    /// </summary>
    [ViewVariables]
    public HashSet<EntityUid> LastAppliedPassable = new();
}
