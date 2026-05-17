using Robust.Shared.GameStates;

namespace Content.Shared._DV.NodeCrawl;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedNodeCrawlSystem))]
public sealed partial class NodeCrawlComponent : Component
{
    [DataField, AutoNetworkedField]
    public HashSet<EntityUid> ReachableNodes = new();
}
