using Content.Server._DV.Grappling.EntitySystems;
using Content.Server.Interaction;

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators.Specific;

/// <summary>
/// Handles an NPC attempting to perform the dog whistles' Catch order,
/// which grapples an entity.
/// </summary>
public sealed partial class CatchOrderOperator : HTNOperator
{
    private GrapplingSystem _grappling = default!;
    private InteractionSystem _interaction = default!;

    /// <summary>
    /// Range this entity must be to the target to consider grappling.
    /// </summary>
    [DataField]
    private string _distanceKey = "CatchRange";

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);

        _grappling = sysManager.GetEntitySystem<GrapplingSystem>();
        _interaction = sysManager.GetEntitySystem<InteractionSystem>();
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        var target = blackboard.GetValue<EntityUid>(NPCBlackboard.CurrentOrderedTarget);
        var range = blackboard.GetValue<float>(_distanceKey);

        if (!_interaction.InRangeUnobstructed(owner, target, range))
            return HTNOperatorStatus.Continuing; // Move towards the target.

        if (!_grappling.TryStartGrapple(owner, target))
            return HTNOperatorStatus.Failed; // Couldn't grapple for some reason.

        return HTNOperatorStatus.Finished;
    }
}
