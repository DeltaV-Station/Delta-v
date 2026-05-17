using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.NodeCrawl;

/// <summary>
/// Handles entities that can enter and exit node-constrained movement.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
[Access(typeof(SharedNodeCrawlSystem))]
public sealed partial class NodeCrawlerComponent : Component
{
    /// <summary>
    /// The mover this crawler is currently being carried by, if any
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Mover;

    /// <summary>
    /// Components of entities to reveal while inside a mover
    /// </summary>
    [DataField]
    public Type[] RevealedComponents;

    /// <summary>
    /// Whitelist for entities that will be considered as exit nodes.
    /// </summary>
    [DataField]
    public EntityWhitelist? ExitNodes;
}
