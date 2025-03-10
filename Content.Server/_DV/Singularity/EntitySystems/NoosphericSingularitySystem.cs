using Content.Server.Physics.Components;
using Content.Server._DV.Singularity.Components;
using Content.Server.Singularity.Events;
using Content.Shared._DV.Singularity.Components;
using Content.Shared.Singularity.EntitySystems;
using Content.Shared.Singularity.Events;
using Robust.Server.GameStates;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Content.Shared._DV.Noospherics;
using Robust.Shared.Random;
using Content.Server.Singularity.Components;

namespace Content.Server._DV.Singularity.EntitySystems;

public sealed class NoosphericSingularitySystem : SharedSingularitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

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
        SubscribeLocalEvent<NoosphericFoodComponent, EventHorizonConsumedEntityEvent>(OnConsumed);
        SubscribeLocalEvent<NoosphericSingularityComponent, EntityConsumedByEventHorizonEvent>(OnConsumedEntity);
        SubscribeLocalEvent<NoosphericSingularityComponent, TilesConsumedByEventHorizonEvent>(OnConsumedTiles);
        SubscribeLocalEvent<NoosphericSingularityComponent, SingularityLevelChangedEvent>(UpdateEnergyDrain);
        /*
        SubscribeLocalEvent<NoosphericSingularityComponent, ComponentGetState>(HandleSingularityState);

        // TODO: Figure out where all this coupling should be handled.
        // TODO: Don't inherit TODOs from other things
        SubscribeLocalEvent<RandomWalkComponent, SingularityLevelChangedEvent>(UpdateRandomWalk);
        SubscribeLocalEvent<GravityWellComponent, SingularityLevelChangedEvent>(UpdateGravityWell);
        **/

        var vvHandle = Vvm.GetTypeHandler<NoosphericSingularityComponent>();
        vvHandle.AddPath(nameof(NoosphericSingularityComponent.Energy), (_, comp) => comp.Energy, SetEnergy);
        vvHandle.AddPath(nameof(NoosphericSingularityComponent.TargetUpdatePeriod),
            (_, comp) => comp.TargetUpdatePeriod,
            SetUpdatePeriod);
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
        var deltaValue = -singularity.EnergyDrain * (float)frameTime.TotalSeconds;

        var delta = new Dictionary<ParticleType, float>()
        {
            { ParticleType.Delta, deltaValue },
            { ParticleType.Epsilon, deltaValue },
            { ParticleType.Omega, deltaValue },
            { ParticleType.Zeta, deltaValue },
        };

        AdjustEnergy(uid, delta, singularity: singularity);
    }

    public void OnSingularityShutdown(Entity<NoosphericSingularityComponent> ent, ref ComponentShutdown args)
    {
        ent.Comp.AmbientSoundStream = _audio.Stop(ent.Comp.AmbientSoundStream);

        MetaDataComponent? metaData = null;
        if (Resolve(ent.Owner, ref metaData) && metaData.EntityLifeStage >= EntityLifeStage.Terminating)
        {
            var xform = Transform(ent.Owner);
            var coordinates = xform.Coordinates;

            // I feel like IsValid should be checking this or something idk.
            if (!TerminatingOrDeleted(coordinates.EntityId))
                _audio.PlayPvs(ent.Comp.DissipationSound, coordinates);
        }
    }


    /// <summary>
    /// Adds the energy of any entities that are consumed to the singularity that consumed them.
    /// </summary>
    /// <param name="uid">The entity UID of the singularity that is consuming the entity.</param>
    /// <param name="comp">The component of the singularity that is consuming the entity.</param>
    /// <param name="args">The event arguments.</param>
    public void OnConsumedEntity(Entity<NoosphericSingularityComponent> ent, ref EntityConsumedByEventHorizonEvent args)
    {
        var type = (ParticleType)_random.Next(0, Enum.GetValues<ParticleType>().Length + 1);
        AdjustEnergy(ent, type, BaseEntityEnergy, singularity: ent.Comp);
    }

    /// <summary>
    /// Adds the energy of any tiles that are consumed to the singularity that consumed them.
    /// </summary>
    /// <param name="uid">The entity UID of the singularity that is consuming the tiles.</param>
    /// <param name="comp">The component of the singularity that is consuming the tiles.</param>
    /// <param name="args">The event arguments.</param>
    public void OnConsumedTiles(Entity<NoosphericSingularityComponent> ent, ref TilesConsumedByEventHorizonEvent args)
    {
        var type = (ParticleType)_random.Next(0, Enum.GetValues<ParticleType>().Length + 1);
        AdjustEnergy(ent, type, args.Tiles.Count * BaseTileEnergy, singularity: ent.Comp);
    }

    /// <summary>
    /// Adds the energy of this singularity to singularities that consume it.
    /// </summary>
    /// <param name="uid">The entity UID of the singularity that is being consumed.</param>
    /// <param name="comp">The component of the singularity that is being consumed.</param>
    /// <param name="args">The event arguments.</param>
    private void OnConsumed(Entity<NoosphericSingularityComponent> ent,
        ref EventHorizonConsumedEntityEvent args)
    {
        if (EntityManager.TryGetComponent<NoosphericSingularityComponent>(args.EventHorizonUid, out var singulo))
        {
            AdjustEnergy(args.EventHorizonUid, ent.Comp.Energy, singularity: singulo);
            SetAllEnergy(ent.Owner, 0.0f, ent.Comp);
        }
    }

    /// <summary>
    /// Adds some bonus energy from any singularity food to the singularity that consumes it.
    /// </summary>
    /// <param name="uid">The entity UID of the singularity food that is being consumed.</param>
    /// <param name="comp">The component of the singularity food that is being consumed.</param>
    /// <param name="args">The event arguments.</param>
    public void OnConsumed(Entity<NoosphericFoodComponent> ent,
        ref EventHorizonConsumedEntityEvent args)
    {
        if (EntityManager.TryGetComponent<NoosphericSingularityComponent>(args.EventHorizonUid, out var singulo))
            AdjustEnergy(args.EventHorizonUid, ent.Comp.Particles, singularity: singulo);
    }

    public void AdjustEnergy(EntityUid uid,
        Dictionary<ParticleType, float> delta,
        float min = float.MinValue,
        float max = float.MaxValue,
        bool snapMin = true,
        bool snapMax = true,
        NoosphericSingularityComponent? singularity = null)
    {
        if (!Resolve(uid, ref singularity))
            return;

        foreach (var type in Enum.GetValues<ParticleType>())
        {
            AdjustEnergy(uid, type, delta[type], min, max, snapMin, snapMax, singularity);
        }
    }

    public void AdjustEnergy(EntityUid uid,
        ParticleType type,
        float delta,
        float min = float.MinValue,
        float max = float.MaxValue,
        bool snapMin = true,
        bool snapMax = true,
        NoosphericSingularityComponent? singularity = null)
    {
        if (!Resolve(uid, ref singularity))
            return;

        var newValue = singularity.Energy[type] + delta;
        if (!snapMin && newValue < min)
            return;

        if (!snapMax && newValue > max)
            return;

        SetEnergy(uid, type, MathHelper.Clamp(newValue, min, max), singularity);
    }

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

    public void UpdateEnergyDrain(Entity<NoosphericSingularityComponent> ent, ref SingularityLevelChangedEvent args)
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

    public void SetAllEnergy(EntityUid uid, float value, NoosphericSingularityComponent? singularity = null)
    {
        if (!Resolve(uid, ref singularity))
            return;

        foreach (var type in Enum.GetValues<ParticleType>())
        {
            singularity.Energy[type] = value;
        }
    }

    public void SetEnergy(EntityUid uid,
        Dictionary<ParticleType, float> energy,
        NoosphericSingularityComponent? singularity = null)
    {
        if (!Resolve(uid, ref singularity))
            return;

        foreach (var type in Enum.GetValues<ParticleType>())
        {
            SetEnergy(uid, type, energy[type]);
        }

        // TODO: Consider re-adding setLevel
        /*
        SetLevel(uid,
            value switch
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
        */
    }

    public void SetEnergy(EntityUid uid,
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

        // TODO: Consider re-adding setLevel
        /*
        SetLevel(uid,
            value switch
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
        */
    }
}
