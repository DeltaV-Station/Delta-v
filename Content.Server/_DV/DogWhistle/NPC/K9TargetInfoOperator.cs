using System.Threading;
using System.Threading.Tasks;
using Content.Server._DV.Grappling.EntitySystems;
using Content.Shared.Mobs.Components;

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators.Specific;

/// <summary>
/// Sets up target information for later usage by the K9Compound.
/// </summary>
public sealed partial class K9TargetInfoOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private GrapplingSystem _grappling = default!;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);

        _grappling = sysManager.GetEntitySystem<GrapplingSystem>();
    }

    /// <summary>
    /// Sets additional blackboard entries for whether the target is grappable, and is a mob.
    /// </summary>
    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(NPCBlackboard blackboard,
        CancellationToken cancelToken)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        var target = blackboard.GetValue<EntityUid>(NPCBlackboard.CurrentOrderedTarget);

        return (true,
            new Dictionary<string, object>
            {
                { "HasGrappleTarget", _grappling.CanGrapple(owner, target) },
                { "HasMobTarget", _entManager.HasComponent<MobStateComponent>(target) }
            }
        );
    }
}
