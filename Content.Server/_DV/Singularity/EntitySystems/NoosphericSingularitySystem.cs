using Content.Server._DV.Singularity.Components;
using Content.Server.Physics.Components;
using Content.Server.Singularity.Components;
using Content.Server.Singularity.Events;
using Content.Shared._DV.Noospherics;
using Content.Shared._DV.Singularity.Components;
using Content.Shared._DV.Singularity.EntitySystems;
using Content.Shared._DV.Singularity.Events;
using Content.Shared.Singularity.Components;
using Robust.Server.GameStates;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._DV.Singularity.EntitySystems;

/// <summary>
/// The server-side version of <see cref="SharedNoosphericSingularitySystem"/>.
/// Primarily responsible for managing <see cref="NoosphericSingularityComponent"/>s.
/// Handles their accumulation of energy upon consuming entities (see <see cref="EventHorizonComponent"/>) and gradual dissipation.
/// Also handles synchronizing server-side components with the singuarities level.
/// </summary>
public sealed class SingularitySystem : SharedNoosphericSingularitySystem
{
    #region Dependencies

    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly PvsOverrideSystem _pvs = default!;

    #endregion Dependencies

    /// <summary>
    /// The amount of energy singulos accumulate when they eat a tile.
    /// </summary>
    public const float BaseTileEnergy = 1f;

    /// <summary>
    /// The amount of energy singulos accumulate when they eat an entity.
    /// </summary>
    public const float BaseEntityEnergy = 1f;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NoosphericSingularityComponent, ComponentShutdown>(OnSingularityShutdown);
        SubscribeLocalEvent<NoosphericSingularityComponent, EventHorizonConsumedEntityEvent>(OnConsumed);
        SubscribeLocalEvent<NoosphericSinguloFoodComponent, EventHorizonConsumedEntityEvent>(OnConsumed);
        SubscribeLocalEvent<NoosphericSingularityComponent, EntityConsumedByEventHorizonEvent>(OnConsumedEntity);
        SubscribeLocalEvent<NoosphericSingularityComponent, TilesConsumedByEventHorizonEvent>(OnConsumedTiles);
        SubscribeLocalEvent<NoosphericSingularityComponent, NoosphericSingularityLevelChangedEvent>(UpdateEnergyDrain);
        SubscribeLocalEvent<NoosphericSingularityComponent, ComponentGetState>(HandleSingularityState);

        // TODO: Figure out where all this coupling should be handled.
        SubscribeLocalEvent<RandomWalkComponent, NoosphericSingularityLevelChangedEvent>(UpdateRandomWalk);
        SubscribeLocalEvent<GravityWellComponent, NoosphericSingularityLevelChangedEvent>(UpdateGravityWell);

        var vvHandle = Vvm.GetTypeHandler<NoosphericSingularityComponent>();
        vvHandle.AddPath(nameof(NoosphericSingularityComponent.Energy), (_, comp) => comp.Energy, SetEnergy);
        vvHandle.AddPath(nameof(NoosphericSingularityComponent.TargetUpdatePeriod),
            (_, comp) => comp.TargetUpdatePeriod,
            SetUpdatePeriod);
    }

    public override void Shutdown()
    {
        var vvHandle = Vvm.GetTypeHandler<NoosphericSingularityComponent>();
        vvHandle.RemovePath(nameof(NoosphericSingularityComponent.Energy));
        vvHandle.RemovePath(nameof(NoosphericSingularityComponent.TargetUpdatePeriod));
        base.Shutdown();
    }

    /// <summary>
    /// Handles the gradual dissipation of all singularities.
    /// </summary>
    /// <param name="frameTime">The amount of time since the last set of updates.</param>
    public override void Update(float frameTime)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<NoosphericSingularityComponent>();
        while (query.MoveNext(out var uid, out var singularity))
        {
            var curTime = _timing.CurTime;
            if (singularity.NextUpdateTime <= curTime)
                Update(uid, curTime - singularity.LastUpdateTime, singularity);
        }
    }

    /// <summary>
    /// Handles the gradual energy loss and dissipation of singularity.
    /// </summary>
    /// <param name="uid">The uid of the singularity to update.</param>
    /// <param name="singularity">The state of the singularity to update.</param>
    public void Update(EntityUid uid, NoosphericSingularityComponent? singularity = null)
    {
        if (Resolve(uid, ref singularity))
            Update(uid, _timing.CurTime - singularity.LastUpdateTime, singularity);
    }

    /// <summary>
    /// Handles the gradual energy loss and dissipation of a singularity.
    /// </summary>
    /// <param name="uid">The uid of the singularity to update.</param>
    /// <param name="frameTime">The amount of time that has elapsed since the last update.</param>
    /// <param name="singularity">The state of the singularity to update.</param>
    public void Update(EntityUid uid, TimeSpan frameTime, NoosphericSingularityComponent? singularity = null)
    {
        if (!Resolve(uid, ref singularity))
            return;

        singularity.LastUpdateTime = _timing.CurTime;
        singularity.NextUpdateTime = singularity.LastUpdateTime + singularity.TargetUpdatePeriod;
        var delta = new NoosphericParticleEnergy(-singularity.EnergyDrain * (float)frameTime.TotalSeconds);
        AdjustEnergy(uid, delta, singularity: singularity);
    }

    #region Getters/Setters

    public void SetEnergy(
        EntityUid uid,
        NoosphericParticleEnergy value,
        NoosphericSingularityComponent? singularity = null)
    {
        if (!Resolve(uid, ref singularity))
            return;

        var oldValue = singularity.Energy;
        if (oldValue == value)
            return;

        foreach (var item in singularity.Energy)
        {
            singularity.Energy[item.Key] = value[item.Key];
        }

        var average = singularity.Energy.Average();
        SetLevel(uid,
            average switch
            {
                >= 2400 => 6,
                >= 1600 => 5,
                >= 900 => 4,
                >= 300 => 3,
                >= 200 => 2,
                > 0 => 1,
                _ => 0
            },
            singularity);
    }

    /// <summary>
    /// Setter for <see cref="NoosphericSingularityComponent.Energy"/>.
    /// Also updates the level of the singularity accordingly.
    /// </summary>
    /// <param name="uid">The uid of the singularity to set the energy of.</param>
    /// <param name="value">The amount of energy for the singularity to have.</param>
    /// <param name="singularity">The state of the singularity to set the energy of.</param>
    public void SetEnergy(
        EntityUid uid,
        ParticleType type,
        float value,
        NoosphericSingularityComponent? singularity = null)
    {
        if (!Resolve(uid, ref singularity))
            return;

        var oldValue = singularity.Energy[type];
        if (oldValue == value)
            return;

        singularity.Energy[type] = value;

        var average = singularity.Energy.Average();
        SetLevel(uid,
            average switch
            {
                >= 2400 => 6,
                >= 1600 => 5,
                >= 900 => 4,
                >= 300 => 3,
                >= 200 => 2,
                > 0 => 1,
                _ => 0
            },
            singularity);
    }

    /// <summary>
    /// Adjusts the amount of energy the singularity has accumulated.
    /// </summary>
    /// <param name="uid">The uid of the singularity to adjust the energy of.</param>
    /// <param name="delta">The amount to adjust the energy of the singuarity.</param>
    /// <param name="min">The minimum amount of energy for the singularity to be adjusted to.</param>
    /// <param name="max">The maximum amount of energy for the singularity to be adjusted to.</param>
    /// <param name="snapMin">Whether the amount of energy in the singularity should be forced to within the specified range if it already is below it.</param>
    /// <param name="snapMax">Whether the amount of energy in the singularity should be forced to within the specified range if it already is above it.</param>
    /// <param name="singularity">The state of the singularity to adjust the energy of.</param>
    public void AdjustEnergy(EntityUid uid,
        NoosphericParticleEnergy delta,
        float min = float.MinValue,
        float max = float.MaxValue,
        bool snapMin = true,
        bool snapMax = true,
        NoosphericSingularityComponent? singularity = null)
    {
        if (!Resolve(uid, ref singularity))
            return;

        var newValue = singularity.Energy + delta;
        foreach (var item in newValue)
        {
            if (!snapMin && newValue[item.Key] < min)
                continue;
            if (!snapMax && newValue[item.Key] > max)
                continue;

            SetEnergy(uid, item.Key, MathHelper.Clamp(newValue[item.Key], min, max), singularity);
        }
    }

    /// <summary>
    /// Setter for <see cref="NoosphericSingularityComponent.TargetUpdatePeriod"/>.
    /// If the new target time implies that the singularity should have updated it does so immediately.
    /// </summary>
    /// <param name="uid">The uid of the singularity to set the update period for.</param>
    /// <param name="value">The new update period for the singularity.</param>
    /// <param name="singularity">The state of the singularity to set the update period for.</param>
    public void SetUpdatePeriod(EntityUid uid, TimeSpan value, NoosphericSingularityComponent? singularity = null)
    {
        if (!Resolve(uid, ref singularity))
            return;

        if (MathHelper.CloseTo(singularity.TargetUpdatePeriod.TotalSeconds, value.TotalSeconds))
            return;

        singularity.TargetUpdatePeriod = value;
        singularity.NextUpdateTime = singularity.LastUpdateTime + singularity.TargetUpdatePeriod;

        var curTime = _timing.CurTime;
        if (singularity.NextUpdateTime <= curTime)
            Update(uid, curTime - singularity.LastUpdateTime, singularity);
    }

    #endregion Getters/Setters

    #region Event Handlers

    /// <summary>
    /// Handles playing the startup sounds when a singulo forms.
    /// Always sets up the ambient singularity rumble.
    /// The formation sound only plays if the singularity is being created.
    /// </summary>
    /// <param name="uid">The entity UID of the singularity that is forming.</param>
    /// <param name="comp">The component of the singularity that is forming.</param>
    /// <param name="args">The event arguments.</param>
    protected override void OnSingularityStartup(Entity<NoosphericSingularityComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.LastUpdateTime = _timing.CurTime;
        ent.Comp.NextUpdateTime = ent.Comp.LastUpdateTime + ent.Comp.TargetUpdatePeriod;

        MetaDataComponent? metaData = null;
        if (Resolve(ent, ref metaData) && metaData.EntityLifeStage <= EntityLifeStage.Initializing)
            _audio.PlayPvs(ent.Comp.FormationSound, ent);

        ent.Comp.AmbientSoundStream = _audio.PlayPvs(ent.Comp.AmbientSound, ent)?.Entity;
        UpdateSingularityLevel(ent.AsNullable());
    }

    /// <summary>
    /// Handles playing the shutdown sounds when a singulo dissipates.
    /// Always stops the ambient singularity rumble.
    /// The dissipations sound only plays if the singularity is being destroyed.
    /// </summary>
    /// <param name="uid">The entity UID of the singularity that is dissipating.</param>
    /// <param name="comp">The component of the singularity that is dissipating.</param>
    /// <param name="args">The event arguments.</param>
    public void OnSingularityShutdown(Entity<NoosphericSingularityComponent> ent, ref ComponentShutdown args)
    {
        ent.Comp.AmbientSoundStream = _audio.Stop(ent.Comp.AmbientSoundStream);

        MetaDataComponent? metaData = null;
        if (Resolve(ent, ref metaData) && metaData.EntityLifeStage >= EntityLifeStage.Terminating)
        {
            var xform = Transform(ent);
            var coordinates = xform.Coordinates;

            // I feel like IsValid should be checking this or something idk.
            if (!TerminatingOrDeleted(coordinates.EntityId))
                _audio.PlayPvs(ent.Comp.DissipationSound, coordinates);
        }
    }

    /// <summary>
    /// Handles wrapping the state of a singularity for server-client syncing.
    /// </summary>
    /// <param name="uid">The uid of the singularity that is being synced.</param>
    /// <param name="comp">The state of the singularity that is being synced.</param>
    /// <param name="args">The event arguments.</param>
    private void HandleSingularityState(Entity<NoosphericSingularityComponent> ent, ref ComponentGetState args)
    {
        args.State = new NoosphericSingularityComponentState(ent);
    }

    /// <summary>
    /// Adds the energy of any entities that are consumed to the singularity that consumed them.
    /// </summary>
    /// <param name="uid">The entity UID of the singularity that is consuming the entity.</param>
    /// <param name="comp">The component of the singularity that is consuming the entity.</param>
    /// <param name="args">The event arguments.</param>
    public void OnConsumedEntity(
        Entity<NoosphericSingularityComponent> ent,
        ref EntityConsumedByEventHorizonEvent args)
    {
        AdjustEnergy(ent, new NoosphericParticleEnergy(BaseEntityEnergy));
    }

    /// <summary>
    /// Adds the energy of any tiles that are consumed to the singularity that consumed them.
    /// </summary>
    /// <param name="uid">The entity UID of the singularity that is consuming the tiles.</param>
    /// <param name="comp">The component of the singularity that is consuming the tiles.</param>
    /// <param name="args">The event arguments.</param>
    public void OnConsumedTiles(
        Entity<NoosphericSingularityComponent> ent,
        ref TilesConsumedByEventHorizonEvent args)
    {
        AdjustEnergy(ent, new NoosphericParticleEnergy(args.Tiles.Count * BaseTileEnergy), singularity: ent.Comp);
    }

    /// <summary>
    /// Adds the energy of this singularity to singularities that consume it.
    /// </summary>
    /// <param name="uid">The entity UID of the singularity that is being consumed.</param>
    /// <param name="comp">The component of the singularity that is being consumed.</param>
    /// <param name="args">The event arguments.</param>
    private void OnConsumed(
        Entity<NoosphericSingularityComponent> ent,
        ref EventHorizonConsumedEntityEvent args)
    {
        // Should be slightly more efficient than checking literally everything we consume for a singularity component and doing the reverse.
        if (EntityManager.TryGetComponent<NoosphericSingularityComponent>(args.EventHorizonUid, out var singulo))
        {
            AdjustEnergy(args.EventHorizonUid, ent.Comp.Energy, singularity: singulo);
            SetEnergy(ent, new NoosphericParticleEnergy(), ent.Comp);
        }
    }

    /// <summary>
    /// Adds some bonus energy from any singularity food to the singularity that consumes it.
    /// </summary>
    /// <param name="uid">The entity UID of the singularity food that is being consumed.</param>
    /// <param name="comp">The component of the singularity food that is being consumed.</param>
    /// <param name="args">The event arguments.</param>
    public void OnConsumed(Entity<NoosphericSinguloFoodComponent> ent, ref EventHorizonConsumedEntityEvent args)
    {
        if (EntityManager.TryGetComponent<NoosphericSingularityComponent>(args.EventHorizonUid, out var singulo))
        {
            // Calculate the percentage change (positive or negative)
            var percentageChange = singulo.Energy * (ent.Comp.EnergyFactor - 1f);
            // Apply both the flat and percentage changes
            AdjustEnergy(args.EventHorizonUid, ent.Comp.Energy + percentageChange, singularity: singulo);
        }
    }

    /// <summary>
    /// Updates the rate at which the singularities energy drains at when its level changes.
    /// </summary>
    /// <param name="uid">The entity UID of the singularity that changed in level.</param>
    /// <param name="comp">The component of the singularity that changed in level.</param>
    /// <param name="args">The event arguments.</param>
    public void UpdateEnergyDrain(
        Entity<NoosphericSingularityComponent> ent,
        ref NoosphericSingularityLevelChangedEvent args)
    {
        ent.Comp.EnergyDrain = args.NewValue switch
        {
            6 => 20,
            5 => 15,
            4 => 12,
            3 => 8,
            2 => 2,
            1 => 1,
            _ => 0
        };
    }

    /// <summary>
    /// Updates the possible speeds of the singulos random walk when the singularities level changes.
    /// </summary>
    /// <param name="uid">The entity UID of the singularity.</param>
    /// <param name="comp">The random walk component component sharing the entity with the singulo component.</param>
    /// <param name="args">The event arguments.</param>
    private void UpdateRandomWalk(Entity<RandomWalkComponent> ent, ref NoosphericSingularityLevelChangedEvent args)
    {
        var scale = MathF.Max(args.NewValue, 4);
        ent.Comp.MinSpeed = 7.5f / scale;
        ent.Comp.MaxSpeed = 10f / scale;
    }

    /// <summary>
    /// Updates the size and strength of the singularities gravity well when the singularities level changes.
    /// </summary>
    /// <param name="uid">The entity UID of the singularity.</param>
    /// <param name="comp">The gravity well component sharing the entity with the singulo component.</param>
    /// <param name="args">The event arguments.</param>
    private void UpdateGravityWell(
        Entity<GravityWellComponent> ent,
        ref NoosphericSingularityLevelChangedEvent args)
    {
        var singulos = args.Singularity;
        ent.Comp.MaxRange = GravPulseRange(singulos);
        (ent.Comp.BaseRadialAcceleration, ent.Comp.BaseTangentialAcceleration) = GravPulseAcceleration(singulos);
    }

    #endregion Event Handlers
}
