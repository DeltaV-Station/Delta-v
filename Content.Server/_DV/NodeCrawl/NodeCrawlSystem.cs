using Content.Server.NodeContainer.EntitySystems;
using Content.Shared._DV.NodeCrawl;
using Content.Shared.NodeContainer;

namespace Content.Server._DV.NodeCrawl;

public sealed class NodeCrawlSystem : SharedNodeCrawlSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NodeCrawlComponent, NodeGroupsRebuilt>(OnNodeGroupsRebuilt);
    }

    private void OnNodeGroupsRebuilt(Entity<NodeCrawlComponent> ent, ref NodeGroupsRebuilt args)
    {
        if (!TryComp<NodeContainerComponent>(ent, out var nodeContainer))
            return;

        var set = new HashSet<EntityUid>();
        foreach (var node in nodeContainer.Nodes.Values)
        {
            foreach (var reachable in node.ReachableNodes)
            {
                set.Add(reachable.Owner);
            }
        }

        ent.Comp.ReachableNodes = set;
        Dirty(ent);
    }
}
