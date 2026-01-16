using System.Linq;
using Content.Shared.Xenoarchaeology.Artifact.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Xenoarchaeology.Artifact;

public abstract partial class SharedXenoArtifactSystem
{

    /// <summary>
    /// Returns the set of nodes that are "related" to the node with the passed node index.
    /// </summary>
    /// <remarks>
    /// Related nodes are those that might be triggered together with this node, in order to unlock some node.
    /// Triggering nodes in unrelated parts of the graph (e.g. different segments, or diverging branches)
    /// causes the unlocking phase to fail. (see TryGetNodeFromUnlockState)
    /// </remarks>
    public HashSet<int> GetRelatedNodes(Entity<XenoArtifactComponent?> ent, int nodeIdx)
    {
        if (!Resolve(ent, ref ent.Comp))
            return new();

        var related = GetRelatedNodes(ent, GetNode((ent, ent.Comp), nodeIdx));
        var output = new HashSet<int>();
        foreach (var r in related)
        {
            output.Add(GetIndex((ent, ent.Comp), r));
        }

        return output;
    }

    /// <summary>
    /// Returns set of node entities, that are "related" to passed node entity.
    /// </summary>
    /// <remarks>
    /// Related nodes are those that might be triggered together with this node, in order to unlock some node.
    /// Triggering nodes in unrelated parts of the graph (e.g. different segments, or diverging branches)
    /// causes the unlocking phase to fail. (see TryGetNodeFromUnlockState)
    /// </remarks>
    public HashSet<Entity<XenoArtifactNodeComponent>> GetRelatedNodes(Entity<XenoArtifactComponent?> ent, Entity<XenoArtifactNodeComponent> node)
    {
        if (!Resolve(ent, ref ent.Comp))
            return new();

        var potentialUnlockTargetNodes = GetSuccessorNodes(ent, node);
        potentialUnlockTargetNodes.Add(node);

        var output = new HashSet<Entity<XenoArtifactNodeComponent>>();
        foreach (var t in potentialUnlockTargetNodes)
        {
            var tPredecessors = GetPredecessorNodes(ent, t);
            // nodes can only be unlocked if all their predecessors are unlocked
            if (tPredecessors.All(p => !p.Comp.Locked))
            {
                output.Add(t);
                foreach (var p in tPredecessors)
                {
                    output.Add(p);
                }
            }
        }

        return output;
    }

}
