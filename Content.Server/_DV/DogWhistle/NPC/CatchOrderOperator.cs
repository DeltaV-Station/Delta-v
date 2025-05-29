using System.Threading;
using System.Threading.Tasks;
using Content.Server._DV.Grappling.EntitySystems;
using Content.Server.Interaction;

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators.Specific;

/// <summary>
/// Handles an NPC attempting to perform the dog whistles' Catch order,
/// which grapples an entity.
/// </summary>
public sealed partial class CatchOrderOperator : HTNOperator
{
    private GrapplingSystem _grapplingSystem = default!;
    private InteractionSystem _interactionSystem = default!;

    /// <summary>
    /// Range this entity must be to the target to consider grappling.
    /// </summary>
    [DataField]
    private string _distanceKey = "CatchRange";

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);

        _grapplingSystem = sysManager.GetEntitySystem<GrapplingSystem>();
        _interactionSystem = sysManager.GetEntitySystem<InteractionSystem>();
    }

    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(NPCBlackboard blackboard,
        CancellationToken cancelToken)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        var target = blackboard.GetValue<EntityUid>(NPCBlackboard.CurrentOrderedTarget);

        if (!_grapplingSystem.CanGrapple(owner, target))
            return (false, null); // Likely not a grapple target

        return (true, null);
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        var target = blackboard.GetValue<EntityUid>(NPCBlackboard.CurrentOrderedTarget);
        var range = blackboard.GetValue<float>(_distanceKey);

        if (!_interactionSystem.InRangeUnobstructed(owner, target, range))
            return HTNOperatorStatus.Continuing; // Move towards the target.

        if (!_grapplingSystem.TryStartGrapple(owner, target))
            return HTNOperatorStatus.Failed; // Couldn't grapple for some reason.

        return HTNOperatorStatus.Finished;
    }
}
