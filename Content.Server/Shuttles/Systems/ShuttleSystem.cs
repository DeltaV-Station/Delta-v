using Content.Server.Administration.Logs;
using Content.Server.Body.Systems;
<<<<<<< HEAD
using Content.Server.Doors.Systems;
=======
using Content.Server.Buckle.Systems;
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
using Content.Server.Parallax;
using Content.Server.Procedural;
using Content.Server.Shuttles.Components;
using Content.Server.Station.Systems;
using Content.Server.Stunnable;
<<<<<<< HEAD
using Content.Shared.GameTicking;
using Content.Shared.Mobs.Systems;
=======
using Content.Shared.Buckle.Components;
using Content.Shared.Damage;
using Content.Shared.Light.Components;
using Content.Shared.Movement.Events;
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
using Content.Shared.Salvage;
using Content.Shared.Shuttles.Systems;
using Content.Shared.Throwing;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Server.GameStates;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Shuttles.Systems;

[UsedImplicitly]
public sealed partial class ShuttleSystem : SharedShuttleSystem
{
    [Dependency] private readonly IAdminLogManager _logger = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefManager = default!;
    [Dependency] private readonly BiomeSystem _biomes = default!;
    [Dependency] private readonly BodySystem _bobby = default!;
    [Dependency] private readonly DockingSystem _dockSystem = default!;
    [Dependency] private readonly DungeonSystem _dungeon = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
<<<<<<< HEAD
    [Dependency] private readonly FixtureSystem _fixtures = default!;
=======
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
    [Dependency] private readonly MapLoaderSystem _loader = default!;
    [Dependency] private readonly MetaDataSystem _metadata = default!;
    [Dependency] private readonly PvsOverrideSystem _pvs = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedSalvageSystem _salvage = default!;
    [Dependency] private readonly ShuttleConsoleSystem _console = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly StunSystem _stuns = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly ThrusterSystem _thruster = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;

    private EntityQuery<MapGridComponent> _gridQuery;

<<<<<<< HEAD
    public const float TileMassMultiplier = 0.5f;

=======
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
    public override void Initialize()
    {
        base.Initialize();

        _gridQuery = GetEntityQuery<MapGridComponent>();

        InitializeFTL();
        InitializeGridFills();
        InitializeIFF();
        InitializeImpact();

        SubscribeLocalEvent<ShuttleComponent, ComponentStartup>(OnShuttleStartup);
        SubscribeLocalEvent<ShuttleComponent, ComponentShutdown>(OnShuttleShutdown);

        SubscribeLocalEvent<GridInitializeEvent>(OnGridInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        UpdateHyperspace();
    }

<<<<<<< HEAD
    private void OnGridFixtureChange(EntityUid uid, FixturesComponent manager, GridFixtureChangeEvent args)
    {
        foreach (var fixture in args.NewFixtures)
        {
            _physics.SetDensity(uid, fixture.Key, fixture.Value, TileMassMultiplier, false, manager);
            _fixtures.SetRestitution(uid, fixture.Key, fixture.Value, 0.1f, false, manager);
        }
    }

=======
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
    private void OnGridInit(GridInitializeEvent ev)
    {
        if (HasComp<MapComponent>(ev.EntityUid))
            return;

        EntityManager.EnsureComponent<ShuttleComponent>(ev.EntityUid);
    }

    private void OnShuttleStartup(EntityUid uid, ShuttleComponent component, ComponentStartup args)
    {
        if (!EntityManager.HasComponent<MapGridComponent>(uid))
        {
            return;
        }

        if (!EntityManager.TryGetComponent(uid, out PhysicsComponent? physicsComponent))
        {
            return;
        }

        if (component.Enabled)
        {
            Enable(uid, component: physicsComponent, shuttle: component);
        }
    }

    public void Toggle(EntityUid uid, ShuttleComponent component)
    {
        if (!EntityManager.TryGetComponent(uid, out PhysicsComponent? physicsComponent))
            return;

        component.Enabled = !component.Enabled;

        if (component.Enabled)
        {
            Enable(uid, component: physicsComponent, shuttle: component);
        }
        else
        {
            Disable(uid, component: physicsComponent);
        }
    }

    public void Enable(EntityUid uid, FixturesComponent? manager = null, PhysicsComponent? component = null, ShuttleComponent? shuttle = null)
    {
        if (!Resolve(uid, ref manager, ref component, ref shuttle, false))
            return;

        _physics.SetBodyType(uid, BodyType.Dynamic, manager: manager, body: component);
        _physics.SetBodyStatus(uid, component, BodyStatus.InAir);
        _physics.SetFixedRotation(uid, false, manager: manager, body: component);
        _physics.SetLinearDamping(uid, component, shuttle.LinearDamping);
        _physics.SetAngularDamping(uid, component, shuttle.AngularDamping);
    }

    public void Disable(EntityUid uid, FixturesComponent? manager = null, PhysicsComponent? component = null)
    {
        if (!Resolve(uid, ref manager, ref component, false))
            return;

        _physics.SetBodyType(uid, BodyType.Static, manager: manager, body: component);
        _physics.SetBodyStatus(uid, component, BodyStatus.OnGround);
        _physics.SetFixedRotation(uid, true, manager: manager, body: component);
    }

    private void OnShuttleShutdown(EntityUid uid, ShuttleComponent component, ComponentShutdown args)
    {
        // None of the below is necessary for any cleanup if we're just deleting.
        if (EntityManager.GetComponent<MetaDataComponent>(uid).EntityLifeStage >= EntityLifeStage.Terminating)
            return;

        Disable(uid);
    }
}
