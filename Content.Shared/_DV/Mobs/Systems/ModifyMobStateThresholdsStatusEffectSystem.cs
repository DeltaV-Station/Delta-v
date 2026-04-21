using Content.Shared._DV.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._DV.Mobs.Systems;

/// <summary>
/// This handles modifying the mobstate thresholds when a status effect modifies them.
/// </summary>
public sealed class ModifyMobStateThresholdsEffectSystem : EntitySystem
{
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ModifyMobStateThresholdsStatusEffectComponent, StatusEffectAppliedEvent>(OnStatusEffectApplied);
        SubscribeLocalEvent<ModifyMobStateThresholdsStatusEffectComponent, StatusEffectRemovedEvent>(OnStatusEffectRemoved);
    }

    private void OnStatusEffectApplied(Entity<ModifyMobStateThresholdsStatusEffectComponent> mob, ref StatusEffectAppliedEvent args)
    {
        foreach (var threshold in mob.Comp.Thresholds)
        {
            var newThreshold = _mobThreshold.GetThresholdForState(args.Target, threshold.Key) + threshold.Value;
            _mobThreshold.SetMobStateThreshold(args.Target, newThreshold, threshold.Key);
        }
    }

    private void OnStatusEffectRemoved(Entity<ModifyMobStateThresholdsStatusEffectComponent> mob, ref StatusEffectRemovedEvent args)
    {
        foreach (var threshold in mob.Comp.Thresholds)
        {
            var newThreshold = _mobThreshold.GetThresholdForState(args.Target, threshold.Key) - threshold.Value;
            _mobThreshold.SetMobStateThreshold(args.Target, newThreshold, threshold.Key);
        }
    }
}
