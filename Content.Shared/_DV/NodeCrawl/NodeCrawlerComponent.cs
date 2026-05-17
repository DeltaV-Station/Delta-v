using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.NodeCrawl;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(NodeCrawlerSystem))]
public sealed partial class NodeCrawlerComponent : Component
{
    /// <summary>
    /// The current node.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Node;

    /// <summary>
    /// The target node being moved to.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? TargetNode;

    /// <summary>
    /// The required angle to be within for deciding which node to move from the target direction
    /// </summary>
    [DataField]
    public double RequiredAngle = Math.PI / 4f;

    /// <summary>
    /// The time between moves.
    /// </summary>
    [DataField]
    public TimeSpan MoveStep = TimeSpan.FromSeconds(0.25f);
}
