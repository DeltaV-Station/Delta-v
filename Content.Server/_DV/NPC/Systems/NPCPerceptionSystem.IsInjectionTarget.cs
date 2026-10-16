using Content.Shared._DV.NPC.Components;

namespace Content.Server.NPC.Systems;

public sealed partial class NPCPerceptionSystem
{
    /// <summary>
    /// Tracks targets currently targeted by medibots.
    /// </summary>
    /// <param name="frameTime"></param>
    private void UpdateIsInjectionTarget(float frameTime)
    {
        var query = EntityQueryEnumerator<NPCIsInjectionTargetComponent>();
        while (query.MoveNext(out var uid, out var entity))
        {
            entity.Accumulator += frameTime;
            if (entity.Accumulator < entity.RemoveTime.TotalSeconds)
                continue;
            entity.Accumulator = 0;

            RemComp<NPCIsInjectionTargetComponent>(uid);
        }
    }
}
