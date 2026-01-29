using Content.Shared._DV.Vision.Components;
using Content.Shared.Examine;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Storage.Components;
using Content.Shared.Traits.Assorted;
using JetBrains.Annotations;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
namespace Content.Shared._DV.Vision.EntitySystems;

/// <summary>
/// This handles granting components to mobs when they observe an entity with the <see cref="GrantComponentsOnObservationComponent"/>.
/// </summary>
public sealed class GrantComponentsOnObservationSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    /// <summary>
    /// Used for processing <see cref="GrantComponentsOnObservationComponent.AffectInContainers"/>.
    /// </summary>
    private EntityQuery<InsideEntityStorageComponent> _insideStorageQuery;

    /// <summary>
    /// Used for processing <see cref="GrantComponentsOnObservationComponent.AffectSilicons"/>.
    /// </summary>
    private EntityQuery<SiliconLawBoundComponent> _siliconLawBoundQuery;

    /// <summary>
    /// Used for processing <see cref="GrantComponentsOnObservationComponent.AffectBlinded"/>.
    /// </summary>
    private EntityQuery<TemporaryBlindnessComponent> _temporaryBlindnessQuery;

    /// <summary>
    /// Used for processing <see cref="GrantComponentsOnObservationComponent.AffectBlinded"/>.
    /// </summary>
    private EntityQuery<PermanentBlindnessComponent> _permanentBlindnessQuery;

    /// <summary>
    /// Used to resolve <see cref="GrantComponentsOnObservationComponent"/> in API calls.
    /// </summary>
    private EntityQuery<GrantComponentsOnObservationComponent> _grantComponentsQuery;

    /// <summary>
    /// Used to process target mobs.
    /// </summary>
    private EntityQuery<MobStateComponent> _mobStateQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        _insideStorageQuery = GetEntityQuery<InsideEntityStorageComponent>();
        _siliconLawBoundQuery = GetEntityQuery<SiliconLawBoundComponent>();
        _temporaryBlindnessQuery = GetEntityQuery<TemporaryBlindnessComponent>();
        _permanentBlindnessQuery = GetEntityQuery<PermanentBlindnessComponent>();
        _grantComponentsQuery = GetEntityQuery<GrantComponentsOnObservationComponent>();
        _mobStateQuery = GetEntityQuery<MobStateComponent>();
    }

    /// <inheritdoc/>
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<GrantComponentsOnObservationComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Grant is null || comp.Grant.Count == 0)
                continue;

            if (comp.NextGrantAttempt.HasValue && comp.NextGrantAttempt.Value > _gameTiming.CurTime)
                continue;

            comp.NextGrantAttempt = _gameTiming.CurTime + comp.Interval;

            if (!comp.AffectInContainers && _insideStorageQuery.HasComp(uid))
                continue;


            var lookup = _entityLookup.GetEntitiesInRange<MobStateComponent>(Transform(uid).Coordinates, comp.Range);

            foreach (var mob in lookup)
            {
                if(!CanEntityAffectTarget((uid, comp), mob.AsNullable()))
                    continue;

                GrantComponents((uid, comp), mob);
            }
        }
    }

    private void GrantComponents(Entity<GrantComponentsOnObservationComponent> entity, Entity<MobStateComponent> target)
    {
        if(entity.Comp.Grant is null || entity.Comp.Grant.Count == 0) return;

        var ev = new ObserverGrantedComponents(entity, target);
        RaiseLocalEvent(target.Owner, ev);

        if(ev.Cancelled) return;

        entity.Comp.AffectedEntities.Add(target.Owner);

        if(entity.Comp.SoundObserver != null)
            _audio.PlayEntity(entity.Comp.SoundObserver, target, target);

        if(entity.Comp.SoundOwner != null)
            _audio.PlayEntity(entity.Comp.SoundOwner, target, entity);

        EntityManager.AddComponents(target.Owner, entity.Comp.Grant, entity.Comp.RemoveExisting);

        if (!string.IsNullOrWhiteSpace(entity.Comp.Message))
            _popup.PopupClient(Loc.GetString(entity.Comp.Message), target.Owner, PopupType.LargeCaution);

        DirtyField(entity.Owner, entity.Comp, nameof(GrantComponentsOnObservationComponent.AffectedEntities));
    }

    /// <summary>
    /// Checks if the entity has granted components to a target mob.
    /// </summary>
    /// <param name="entity">The entity with <see cref="GrantComponentsOnObservationComponent"/></param>
    /// <param name="target">The target mob</param>
    /// <returns>true if the mob has been affected, false otherwise</returns>
    [PublicAPI]
    public bool HasEntityAffectedTarget(Entity<GrantComponentsOnObservationComponent?> entity, EntityUid target)
    {
        return _grantComponentsQuery.Resolve(entity, ref entity.Comp) && entity.Comp.AffectedEntities.Contains(target);
    }

    /// <summary>
    /// Checks if an entity can affect a target mob.
    /// </summary>
    /// <param name="entity">The entity with <see cref="GrantComponentsOnObservationComponent"/></param>
    /// <param name="target">The target mob</param>
    /// <returns>true if the mob can be affected, false otherwise</returns>
    [PublicAPI]
    public bool CanEntityAffectTarget(Entity<GrantComponentsOnObservationComponent?> entity, Entity<MobStateComponent?> target)
    {
        if (!_grantComponentsQuery.Resolve(entity, ref entity.Comp) || !_mobStateQuery.Resolve(target, ref target.Comp))
            return false;

        if (entity.Comp.AffectedEntities.Contains(target.Owner))
            return false;

        if (!entity.Comp.AffectSelf && target.Owner == entity.Owner)
            return false;

        if (target.Comp.CurrentState >= MobState.Critical)
            return false;

        if (!entity.Comp.AffectInContainers && _insideStorageQuery.HasComp(target))
            return false;

        if (!entity.Comp.AffectSilicons && _siliconLawBoundQuery.HasComp(target))
            return false;

        if (!entity.Comp.AffectBlinded && _temporaryBlindnessQuery.HasComp(target) || _permanentBlindnessQuery.HasComp(target))
            return false;

        return _examine.InRangeUnOccluded(entity, target, entity.Comp.Range);
    }
}
