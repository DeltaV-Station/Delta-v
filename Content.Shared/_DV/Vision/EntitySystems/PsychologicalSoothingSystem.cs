using Content.Shared._DV.Vision.Components;
using Content.Shared.Examine;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.Timing;
namespace Content.Shared._DV.Vision.EntitySystems;

/// <summary>
///     This system allows entities to receive soothing from providers based on their proximity.
/// </summary>
public sealed class PsychologicalSoothingSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    /// <summary>
    /// This is used to exclude dead mobs from this system.
    /// </summary>
    private EntityQuery<MobStateComponent> _mobStateQuery;

    public override void Initialize()
    {
        base.Initialize();

        _mobStateQuery = GetEntityQuery<MobStateComponent>();
    }

    public override void Update(float frameTime)
    {
        var receiverQuery = EntityQueryEnumerator<PsychologicalSoothingReceiverComponent>();

        while (receiverQuery.MoveNext(out var entReceiver, out var receiver))
        {
            if (Paused(entReceiver))
                continue;
            
            var providerQuery = _entityLookup.GetEntitiesInRange<PsychologicalSoothingProviderComponent>(Transform(entReceiver).Coordinates, receiver.Range);
            
            if (_mobStateQuery.TryComp(entReceiver, out var mobStateSelf) && mobStateSelf.CurrentState == MobState.Dead)
                continue;

            if (receiver.NextPulse is { } next && _timing.CurTime < next)
                continue;

            receiver.NextPulse = _timing.CurTime + receiver.Interval;
            DirtyField(entReceiver, receiver, nameof(PsychologicalSoothingReceiverComponent.NextPulse));

            var psyDiff = 0f;
            var isBeingSoothed = false;

            foreach (var entProvider in  providerQuery)
            {
                if (_mobStateQuery.TryComp(entProvider, out var mobStateOther) && mobStateOther.CurrentState == MobState.Dead)
                    continue;

                var provider = entProvider.Comp;

                // Not in line of sight, not soothing.
                if (!_examine.InRangeUnOccluded(entReceiver, entProvider, Math.Min(receiver.Range, provider.Range)))
                    continue;

                psyDiff +=  provider.Strength * receiver.RateGrowth;
                isBeingSoothed = true;
            }

            var updatedSoothed = Math.Clamp( receiver.SoothedCurrent + (isBeingSoothed ? psyDiff : -receiver.RateDecay), receiver.SoothedMinimum, receiver.SoothedMaximum );

            // If the soothing isn't changing, then we're done.
            if (MathHelper.CloseTo(receiver.SoothedCurrent, updatedSoothed, 0.00001f))
                continue;

            var ev = new PsychologicalSoothingChanged(updatedSoothed, receiver.SoothedCurrent); // Create the ev before updating the current value so the ev can have current and previous.
            receiver.SoothedCurrent = updatedSoothed;

            RaiseLocalEvent(entReceiver, ref ev);
            DirtyField(entReceiver, receiver, nameof(PsychologicalSoothingReceiverComponent.SoothedCurrent));
        }
    }
}
