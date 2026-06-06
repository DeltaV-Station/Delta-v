using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.StatusEffectNew.Components;

namespace Content.Shared._DV.Psionics.Systems.PsionicPowers;

public abstract class SharedMassSleepPowerSystem : BasePsionicPowerSystem<MassSleepPowerComponent, MassSleepPowerActionEvent>
{
    [Dependency] private readonly SharedBloodstreamSystem _bloodstreamSystem = default!;

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<RegenerativeSleepingStatusEffectComponent, StatusEffectComponent>();
        while (query.MoveNext(out var uid, out var comp, out var statusEffect))
        {
            if (comp.NextTick > Timing.CurTime || statusEffect.AppliedTo is null)
                continue;
            comp.NextTick = Timing.CurTime + TimeSpan.FromSeconds(1); // Metabolism ticks every second.
            Dirty(uid, comp);

            var solution = new Solution();
            solution.AddReagent(comp.ReagentId, comp.Quantity);
            _bloodstreamSystem.TryAddToBloodstream(statusEffect.AppliedTo.Value, solution);
        }
    }
}
