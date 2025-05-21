using System.Linq;
using Content.Server.Doors.Systems;
using Content.Server.Popups;
using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Audio;
using Content.Shared.DoAfter;
using Content.Shared.Doors.Components;
using Content.Shared.Mobs;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._DV.CosmicCult.EntitySystems;

public sealed class CosmicColossusSystem : EntitySystem
{
    [Dependency] private readonly DoorSystem _door = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _move = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedAmbientSoundSystem _ambientSound = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private readonly EntProtoId _tileDetonation = "MobTileDamageArea";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicColossusComponent, MobStateChangedEvent>(OnMobStateChanged);

        SubscribeLocalEvent<CosmicColossusComponent, EventCosmicColossusIngress>(OnColossusIngress);
        SubscribeLocalEvent<CosmicColossusComponent, EventCosmicColossusIngressDoAfter>(OnColossusIngressDoAfter);
        SubscribeLocalEvent<CosmicColossusComponent, EventCosmicColossusSunder>(OnColossusSunder);

    }

    private void OnMobStateChanged(Entity<CosmicColossusComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState is not MobState.Alive)
        {
            if (!TryComp<PhysicsComponent>(ent, out var physComp))
                return;
            _appearance.SetData(ent, ColossusVisuals.Status, ColossusStatus.Dead);
            _ambientSound.SetAmbience(ent, false);
            _audio.PlayPvs(ent.Comp.DeathSFX, ent);
            _physics.SetBodyStatus(ent, physComp, BodyStatus.OnGround, true);
            _popup.PopupCoordinates(
                Loc.GetString("cosmiccult-colossus-death"),
                Transform(ent).Coordinates,
                PopupType.Large);
            RemComp<PointLightComponent>(ent);
        }
    }

    private void OnColossusIngress(Entity<CosmicColossusComponent> ent, ref EventCosmicColossusIngress args)
    {
        var doargs = new DoAfterArgs(EntityManager, ent, ent.Comp.IngressDoAfter, new EventCosmicColossusIngressDoAfter(), ent, args.Target)
        {
            DistanceThreshold = 2f,
            Hidden = false,
            BreakOnMove = true,
        };
        args.Handled = true;
        _audio.PlayPvs(ent.Comp.DoAfterSFX, ent);
        _doAfter.TryStartDoAfter(doargs);
    }

    private void OnColossusIngressDoAfter(Entity<CosmicColossusComponent> ent, ref EventCosmicColossusIngressDoAfter args)
    {
        if (args.Args.Target is not { } target)
            return;
        if (args.Cancelled || args.Handled)
            return;
        args.Handled = true;
        var comp = ent.Comp;

        if (TryComp<DoorBoltComponent>(target, out var doorBolt))
            _door.SetBoltsDown((target, doorBolt), false);
        _door.StartOpening(target);
        _audio.PlayPvs(comp.IngressSFX, ent);
        Spawn(comp.CultVFX, Transform(target).Coordinates);
    }

    private void OnColossusSunder(Entity<CosmicColossusComponent> ent, ref EventCosmicColossusSunder args)
    {
        _appearance.SetData(ent, ColossusVisuals.Status, ColossusStatus.Attacking);
        _transform.SetCoordinates(ent, args.Target);
        _transform.AnchorEntity(ent);
        _audio.PlayPvs(ent.Comp.TileSFX, ent);

        args.Handled = true;

        Spawn("CosmicColossusAttack1Vfx", args.Target);
        Timer.Spawn(ent.Comp.ReleaseDelay, () => { _appearance.SetData(ent, ColossusVisuals.Status, ColossusStatus.Alive); _transform.Unanchor(ent); });

        var area = Spawn(null, _transform.ToMapCoordinates(args.Target), null, default);
        for (var size = 1; size <= 5; size++)
        {
            var range = size;
            Timer.Spawn(TimeSpan.FromSeconds(size * 0.5), () => { DetonateTiles(area, range); });
        }
        Timer.Spawn(ent.Comp.Cleanup, () => { QueueDel(area); });
    }

    public void DetonateTiles(EntityUid ent, int range = 0)
    {
        var xform = Transform(ent);
        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return;

        var gridEnt = ((EntityUid)xform.GridUid, grid);
        if (!_transform.TryGetGridTilePosition(ent, out var tilePos))
            return;

        var pos = _map.TileCenterToVector(gridEnt, tilePos);
        var bounds = new Box2(pos, pos).Enlarged(range);
        var boundsMod = new Box2(pos, pos).Enlarged(Math.Max(range - 1, 0));
        var zone = _map.GetLocalTilesIntersecting(ent, grid, bounds).ToList();
        var zoneMod = _map.GetLocalTilesIntersecting(ent, grid, boundsMod).ToList();

        zone = zone.Where(b => !zoneMod.Contains(b)).ToList();
        foreach (var tile in zone)
        {
            Spawn(_tileDetonation, _map.GridTileToWorld((EntityUid)xform.GridUid, grid, tile.GridIndices));
        }
    }
}
