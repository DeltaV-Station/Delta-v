using Content.Shared._DV.Vision.Components;
using Content.Shared.Examine;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.Timing;
namespace Content.Shared._DV.Vision.EntitySystems;

/// <summary>
///     This system allows entities to receive soothing from providers based on their proximity.
/// </summary>
public sealed class SharedPsychologicalSoothingSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    /// <summary>
    /// This is used to exclude dead mobs from this system.
    /// </summary>
    private EntityQuery<MobStateComponent> _mobStateQuery = default!;

    public override void Initialize()
    {
        base.Initialize();
        _mobStateQuery = GetEntityQuery<MobStateComponent>();

        SubscribeLocalEvent<PsychologicalSoothingReceiverComponent, ComponentInit>(OnComponentInit);
    }

    private void OnComponentInit(Entity<PsychologicalSoothingReceiverComponent> ent, ref ComponentInit args)
    {
        ent.Comp.SootheNext = _timing.CurTime + ent.Comp.SootheInterval;
    }

    public override void Update(float frameTime)
    {
        var receiverQuery = EntityQueryEnumerator<PsychologicalSoothingReceiverComponent>();

        while (receiverQuery.MoveNext(out var entReceiver, out var receiver))
        {
            var providerQuery = _entityLookup.GetEntitiesInRange<PsychologicalSoothingProviderComponent>(Transform(entReceiver).Coordinates, receiver.Range);

            if (_mobStateQuery.TryComp(entReceiver, out var mobStateSelf) && mobStateSelf.CurrentState == MobState.Dead)
            {
                continue;
            }

            if (!receiver.SootheNext.HasValue || _timing.CurTime < receiver.SootheNext)
            {
                continue;
            }

            receiver.SootheNext = _timing.CurTime + receiver.SootheInterval;
            DirtyField<PsychologicalSoothingReceiverComponent>(entReceiver, nameof(PsychologicalSoothingReceiverComponent.SootheNext));

            var psyDiff = 0f;
            var isBeingSoothed = false;

            foreach (var entProvider in  providerQuery)
            {
                if (_mobStateQuery.TryComp(entProvider, out var mobStateOther) && mobStateOther.CurrentState == MobState.Dead)
                {
                    continue;
                }

                var provider = entProvider.Comp;

                // Not in line of sight, not soothing.
                if (!_examine.InRangeUnOccluded(entReceiver, entProvider, Math.Min(receiver.Range, provider.Range)))
                    continue;

                psyDiff +=  provider.Strength * receiver.RateGrowth;
                isBeingSoothed = true;
            }
            var updatedSoothed = Math.Clamp( receiver.SoothedCurrent + (isBeingSoothed ? psyDiff : -receiver.RateDecay), receiver.SoothedMinimum, receiver.SoothedMaximum );

            if (MathHelper.CloseTo(receiver.SoothedCurrent, updatedSoothed, 0.00001f))
                continue;

            receiver.SoothedCurrent = updatedSoothed;
            DirtyField<PsychologicalSoothingReceiverComponent>(entReceiver, nameof(PsychologicalSoothingReceiverComponent.SoothedCurrent));
        }
    }
}
