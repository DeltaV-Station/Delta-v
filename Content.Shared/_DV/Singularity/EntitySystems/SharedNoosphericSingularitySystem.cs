using System.Numerics;
using Content.Shared.Radiation.Components;
using Content.Shared.Singularity.Components;
using Content.Shared.Singularity.EntitySystems;
using Content.Shared._DV.Singularity.Components;
using Content.Shared._DV.Singularity.Events;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Singularity.EntitySystems;

/// <summary>
/// The entity system primarily responsible for managing <see cref="NoosphericSingularityComponent"/>s.
/// </summary>
public abstract class SharedNoosphericSingularitySystem : EntitySystem
{
    #region Dependencies

    [Dependency] private readonly SharedAppearanceSystem _visualizer = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly SharedEventHorizonSystem _horizons = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] protected readonly IViewVariablesManager Vvm = default!;

    #endregion Dependencies

    /// <summary>
    /// The minimum level a singularity can be set to.
    /// </summary>
    public const byte MinSingularityLevel = 0;

    /// <summary>
    /// The maximum level a singularity can be set to.
    /// </summary>
    public const byte MaxSingularityLevel = 6;

    /// <summary>
    /// The amount to scale a singularities distortion shader by when it's in a container.
    /// This is the inverse of an exponent, not a linear scaling factor.
    /// ie. n => intensity = intensity ** (1/n)
    /// </summary>
    public const float DistortionContainerScaling = 4f;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NoosphericSingularityComponent, ComponentStartup>(OnSingularityStartup);
        SubscribeLocalEvent<AppearanceComponent, NoosphericSingularityLevelChangedEvent>(UpdateAppearance);
        SubscribeLocalEvent<RadiationSourceComponent, NoosphericSingularityLevelChangedEvent>(UpdateRadiation);
        SubscribeLocalEvent<PhysicsComponent, NoosphericSingularityLevelChangedEvent>(UpdateBody);
        SubscribeLocalEvent<EventHorizonComponent, NoosphericSingularityLevelChangedEvent>(UpdateEventHorizon);
        SubscribeLocalEvent<SingularityDistortionComponent, NoosphericSingularityLevelChangedEvent>(UpdateDistortion);
        SubscribeLocalEvent<SingularityDistortionComponent, EntGotInsertedIntoContainerMessage>(UpdateDistortion);
        SubscribeLocalEvent<SingularityDistortionComponent, EntGotRemovedFromContainerMessage>(UpdateDistortion);

        var vvHandle = Vvm.GetTypeHandler<NoosphericSingularityComponent>();
        vvHandle.AddPath(nameof(NoosphericSingularityComponent.Level), (_, comp) => comp.Level, SetLevel);
        vvHandle.AddPath(nameof(NoosphericSingularityComponent.RadsPerLevel),
            (_, comp) => comp.RadsPerLevel,
            SetRadsPerLevel);
    }

    public override void Shutdown()
    {
        var vvHandle = Vvm.GetTypeHandler<NoosphericSingularityComponent>();
        vvHandle.RemovePath(nameof(NoosphericSingularityComponent.Level));
        vvHandle.RemovePath(nameof(NoosphericSingularityComponent.RadsPerLevel));

        base.Shutdown();
    }

    #region Getters/Setters

    /// <summary>
    /// Setter for <see cref="NoosphericSingularityComponent.Level"/>
    /// Also sends out an event alerting that the singularities level has changed.
    /// </summary>
    /// <param name="uid">The uid of the singularity to change the level of.</param>
    /// <param name="value">The new level the singularity should have.</param>
    /// <param name="singularity">The state of the singularity to change the level of.</param>
    public void SetLevel(EntityUid uid, byte value, NoosphericSingularityComponent? singularity = null)
    {
        if (!Resolve(uid, ref singularity))
            return;

        value = MathHelper.Clamp(value, MinSingularityLevel, MaxSingularityLevel);
        var oldValue = singularity.Level;
        if (oldValue == value)
            return;

        singularity.Level = value;
        UpdateSingularityLevel(uid, oldValue, singularity);
        if (!Deleted(uid))
            Dirty(uid, singularity);
    }

    /// <summary>
    /// Setter for <see cref="NoosphericSingularityComponent.RadsPerLevel"/>
    /// Also updates the radiation output of the singularity according to the new values.
    /// </summary>
    /// <param name="uid">The uid of the singularity to change the radioactivity of.</param>
    /// <param name="value">The new radioactivity the singularity should have.</param>
    /// <param name="singularity">The state of the singularity to change the radioactivity of.</param>
    public void SetRadsPerLevel(EntityUid uid, float value, NoosphericSingularityComponent? singularity = null)
    {
        if (!Resolve(uid, ref singularity))
            return;

        var oldValue = singularity.RadsPerLevel;
        if (oldValue == value)
            return;

        singularity.RadsPerLevel = value;
        UpdateRadiation(uid, singularity);
    }

    /// <summary>
    /// Alerts the entity hosting the singularity that the level of the singularity has changed.
    /// Usually follows a SharedSingularitySystem.SetLevel call, but is also used on component startup to sync everything.
    /// </summary>
    /// <param name="uid">The uid of the singularity which's level has changed.</param>
    /// <param name="oldValue">The old level of the singularity. May be equal to <see cref="NoosphericSingularityComponent.Level"/> if the component is starting.</param>
    /// <param name="singularity">The state of the singularity which's level has changed.</param>
    public void UpdateSingularityLevel(EntityUid uid, byte oldValue, NoosphericSingularityComponent? singularity = null)
    {
        if (!Resolve(uid, ref singularity))
            return;

        RaiseLocalEvent(uid, new NoosphericSingularityLevelChangedEvent(singularity.Level, oldValue, singularity));
        if (singularity.Level <= 0)
            QueueDel(uid);
    }

    /// <summary>
    /// Alerts the entity hosting the singularity that the level of the singularity has changed without the level actually changing.
    /// Used to sync components when the singularity component is added to an entity.
    /// </summary>
    /// <param name="uid">The uid of the singularity.</param>
    /// <param name="singularity">The state of the singularity.</param>
    public void UpdateSingularityLevel(Entity<NoosphericSingularityComponent?> ent)
    {
        if (Resolve(ent, ref ent.Comp))
            UpdateSingularityLevel(ent, ent.Comp.Level, ent.Comp);
    }

    /// <summary>
    /// Updates the amount of radiation the singularity emits to reflect a change in the level or radioactivity per level of the singularity.
    /// </summary>
    /// <param name="uid">The uid of the singularity to update the radiation of.</param>
    /// <param name="singularity">The state of the singularity to update the radiation of.</param>
    /// <param name="rads">The state of the radioactivity of the singularity to update.</param>
    private void UpdateRadiation(
        Entity<RadiationSourceComponent?> ent,
        NoosphericSingularityComponent? singularity = null)
    {
        if (!Resolve(ent, ref singularity, ref ent.Comp, logMissing: false))
            return;

        ent.Comp.Intensity = singularity.Level * singularity.RadsPerLevel;
    }

    #endregion Getters/Setters

    #region Derivations

    /// <summary>
    /// The scaling factor for the size of a singularities gravity well.
    /// </summary>
    public const float BaseGravityWellRadius = 2f;

    /// <summary>
    /// The scaling factor for the base acceleration of a singularities gravity well.
    /// </summary>
    public const float BaseGravityWellAcceleration = 10f;

    /// <summary>
    /// The level at and above which a singularity should be capable of breaching containment.
    /// </summary>
    public const byte SingularityBreachThreshold = 5;

    /// <summary>
    /// Derives the proper gravity well radius for a singularity from its state.
    /// </summary>
    /// <param name="singulo">A singularity.</param>
    /// <returns>The gravity well radius the singularity should have given its state.</returns>
    public static float GravPulseRange(NoosphericSingularityComponent singulo)
    {
        return BaseGravityWellRadius * (singulo.Level + 1);
    }

    /// <summary>
    /// Derives the proper base gravitational acceleration for a singularity from its state.
    /// </summary>
    /// <param name="singulo">A singularity.</param>
    /// <returns>The base gravitational acceleration the singularity should have given its state.</returns>
    public static (float, float) GravPulseAcceleration(NoosphericSingularityComponent singulo)
    {
        return (BaseGravityWellAcceleration * singulo.Level, 0f);
    }

    /// <summary>
    /// Derives the proper event horizon radius for a singularity from its state.
    /// </summary>
    /// <param name="singulo">A singularity.</param>
    /// <returns>The event horizon radius the singularity should have given its state.</returns>
    public static float EventHorizonRadius(NoosphericSingularityComponent singulo)
    {
        return singulo.Level - 0.5f;
    }

    /// <summary>
    /// Derives whether a singularity should be able to breach containment from its state.
    /// </summary>
    /// <param name="singulo">A singularity.</param>
    /// <returns>Whether the singularity should be able to breach containment.</returns>
    public static bool CanBreachContainment(NoosphericSingularityComponent singulo)
    {
        return singulo.Level >= SingularityBreachThreshold;
    }

    /// <summary>
    /// Derives the proper distortion shader falloff for a singularity from its state.
    /// </summary>
    /// <param name="singulo">A singularity.</param>
    /// <returns>The distortion shader falloff the singularity should have given its state.</returns>
    public static float GetFalloff(float level)
    {
        return level switch
        {
            0 => 9999f,
            1 => MathF.Sqrt(6.4f),
            2 => MathF.Sqrt(7.0f),
            3 => MathF.Sqrt(8.0f),
            4 => MathF.Sqrt(10.0f),
            5 => MathF.Sqrt(12.0f),
            6 => MathF.Sqrt(12.0f),
            _ => -1.0f
        };
    }

    /// <summary>
    /// Derives the proper distortion shader intensity for a singularity from its state.
    /// </summary>
    /// <param name="singulo">A singularity.</param>
    /// <returns>The distortion shader intensity the singularity should have given its state.</returns>
    public static float GetIntensity(float level)
    {
        return level switch
        {
            0 => 0.0f,
            1 => 3645f,
            2 => 103680f,
            3 => 1113920f,
            4 => 16200000f,
            5 => 180000000f,
            6 => 180000000f,
            _ => -1.0f
        };
    }

    #endregion Derivations

    #region Serialization

    /// <summary>
    /// A state wrapper used to sync the singularity between the server and client.
    /// </summary>
    [Serializable, NetSerializable]
    protected sealed class NoosphericSingularityComponentState(NoosphericSingularityComponent singulo) : ComponentState
    {
        /// <summary>
        /// The level of the singularity to sync.
        /// </summary>
        public readonly byte Level = singulo.Level;
    }

    #endregion Serialization

    #region EventHandlers

    /// <summary>
    /// Syncs other components with the state of the singularity via event on startup.
    /// </summary>
    /// <param name="uid">The entity that is becoming a singularity.</param>
    /// <param name="comp">The singularity component that is being added to the entity.</param>
    /// <param name="args">The event arguments.</param>
    protected virtual void OnSingularityStartup(
        Entity<NoosphericSingularityComponent> ent,
        ref ComponentStartup args)
    {
        UpdateSingularityLevel((ent, ent.Comp));
    }

    // TODO: Figure out which systems should have control of which coupling.
    /// <summary>
    /// Syncs the radius of an event horizon associated with a singularity that just changed levels.
    /// </summary>
    /// <param name="uid">The entity that the event horizon and singularity are attached to.</param>
    /// <param name="comp">The event horizon associated with the singularity.</param>
    /// <param name="args">The event arguments.</param>
    private void UpdateEventHorizon(
        Entity<EventHorizonComponent> ent,
        ref NoosphericSingularityLevelChangedEvent args)
    {
        var singulo = args.Singularity;
        _horizons.SetRadius(ent, EventHorizonRadius(singulo), false, ent.Comp);
        _horizons.SetCanBreachContainment(ent, CanBreachContainment(singulo), false, ent.Comp);
        _horizons.UpdateEventHorizonFixture(ent, eventHorizon: ent.Comp);
    }

    /// <summary>
    /// Updates the distortion shader associated with a singularity when the singuarity changes levels.
    /// </summary>
    /// <param name="uid">The uid of the distortion shader.</param>
    /// <param name="comp">The state of the distortion shader.</param>
    /// <param name="args">The event arguments.</param>
    private void UpdateDistortion(
        Entity<SingularityDistortionComponent> ent,
        ref NoosphericSingularityLevelChangedEvent args)
    {
        var newFalloffPower = GetFalloff(args.NewValue);
        var newIntensity = GetIntensity(args.NewValue);
        if (_containers.IsEntityInContainer(ent))
        {
            // Also handles setting the new falloff on the component
            InternalUpdateDistorion(ent, newFalloffPower, newIntensity, (1f / DistortionContainerScaling) - 1f);
        }
        else
        {
            ent.Comp.FalloffPower = newFalloffPower;
            ent.Comp.Intensity = newIntensity;
        }

        Dirty(ent);
    }

    /// <summary>
    /// Updates the distortion shader associated with a singularity when the singuarity is inserted into a container.
    /// </summary>
    /// <param name="uid">The uid of the distortion shader.</param>
    /// <param name="comp">The state of the distortion shader.</param>
    /// <param name="args">The event arguments.</param>
    private void UpdateDistortion(
        Entity<SingularityDistortionComponent> ent,
        ref EntGotInsertedIntoContainerMessage args)
    {
        InternalUpdateDistorion(ent, ent.Comp.FalloffPower, ent.Comp.Intensity, (1f / DistortionContainerScaling) - 1f);
    }

    /// <summary>
    /// Updates the distortion shader associated with a singularity when the singuarity is removed from a container.
    /// </summary>
    /// <param name="uid">The uid of the distortion shader.</param>
    /// <param name="comp">The state of the distortion shader.</param>
    /// <param name="args">The event arguments.</param>
    private void UpdateDistortion(
        Entity<SingularityDistortionComponent> ent,
        ref EntGotRemovedFromContainerMessage args)
    {
        InternalUpdateDistorion(ent, ent.Comp.FalloffPower, ent.Comp.Intensity, DistortionContainerScaling - 1);
    }

    /// <summary>
    /// Handles updating the components distortion based on a factor and input details
    /// </summary>
    /// <param name="ent">The uid of the distortion shader.</param>
    /// <param name="fallOffPower">Falloff power to use for the shader.</param>
    /// <param name="intensity">Intensity of the shader.</param>
    /// <param name="factor">Factor to use when calculating the new falloff/intensity</param>
    private static void InternalUpdateDistorion(
        Entity<SingularityDistortionComponent> ent,
        float fallOffPower,
        float intensity,
        float factor)
    {
        var absFalloffPower = MathF.Abs(fallOffPower);
        var absIntensity = MathF.Abs(intensity);

        ent.Comp.FalloffPower = absFalloffPower > 1
            ? fallOffPower * MathF.Pow(absFalloffPower, factor)
            : fallOffPower;
        ent.Comp.Intensity =
            absIntensity > 1 ? intensity * MathF.Pow(absIntensity, factor) : intensity;
    }

    /// <summary>
    /// Updates the state of the physics body associated with a singularity when the singualrity changes levels.
    /// </summary>
    /// <param name="uid">The entity that the physics body and singularity are attached to.</param>
    /// <param name="comp">The physics body associated with the singularity.</param>
    /// <param name="args">The event arguments.</param>
    private void UpdateBody(Entity<PhysicsComponent> ent, ref NoosphericSingularityLevelChangedEvent args)
    {
        if (args.NewValue <= 1 &&
            args.OldValue >
            1) // Apparently keeps singularities from getting stuck in the corners of containment fields.
            _physics.SetLinearVelocity(ent,
                Vector2.Zero,
                body: ent.Comp); // No idea how stopping the singularities movement keeps it from getting stuck though.
    }

    /// <summary>
    /// Updates the appearance of a singularity when the singularities level changes.
    /// </summary>
    /// <param name="uid">The entity that the singularity is attached to.</param>
    /// <param name="comp">The appearance associated with the singularity.</param>
    /// <param name="args">The event arguments.</param>
    private void UpdateAppearance(Entity<AppearanceComponent> ent, ref NoosphericSingularityLevelChangedEvent args)
    {
        _visualizer.SetData(ent, NoosphericSingularityAppearanceKeys.Singularity, args.NewValue, ent.Comp);
    }

    /// <summary>
    /// Updates the amount of radiation a singularity emits when the singularities level changes.
    /// </summary>
    /// <param name="uid">The entity that the singularity is attached to.</param>
    /// <param name="comp">The radiation source associated with the singularity.</param>
    /// <param name="args">The event arguments.</param>
    private void UpdateRadiation(
        Entity<RadiationSourceComponent> ent,
        ref NoosphericSingularityLevelChangedEvent args)
    {
        UpdateRadiation((ent, ent.Comp), args.Singularity);
    }

    #endregion EventHandlers
}
