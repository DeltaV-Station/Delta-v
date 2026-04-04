using Content.Server._DV.Grappling.EntitySystems;

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators.Specific;

/// <summary>
/// Handles an NPC attempting to drop any caught entities from a previous
/// <see cref="CatchOrderOperator"/>.
/// </summary>
public sealed partial class DropCatchOrderOperator : HTNOperator
{
    private GrapplingSystem _grappling = default!;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);

        _grappling = sysManager.GetEntitySystem<GrapplingSystem>();
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!_grappling.IsGrappling(owner))
            return HTNOperatorStatus.Finished; // Weren't grappling anything, so success.

        return _grappling.ReleaseGrapple(owner, manualRelease: true)
            ? HTNOperatorStatus.Finished
            : HTNOperatorStatus.Failed;
    }
}
