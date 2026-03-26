using System.Numerics;
using Content.Server._DV.CosmicCult.Components;
using Content.Server.Actions;
using Content.Server.Chat.Managers;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Server.Popups;
using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Anomaly.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Maps;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Content.Shared.Warps;
using Content.Shared.Weapons.Melee;
using Robust.Server.Audio;
using Robust.Server.Player;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicEffigySystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly CodeConditionSystem _codeCondition = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly CosmicCultObjectiveSystem _cultObjective = default!;
    [Dependency] private readonly IGameTiming _time = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicColossusComponent, EventCosmicColossusEffigy>(OnColossusEffigy);
        SubscribeLocalEvent<CosmicEffigyComponent, AnomalySupercriticalEvent>(OnSupercritical);
        SubscribeLocalEvent<CosmicEffigyComponent, AnomalyShutdownEvent>(OnAnomShutdown);
    }

    private void OnAnomShutdown(Entity<CosmicEffigyComponent> ent, ref AnomalyShutdownEvent args)
    {
        if (args.Forced || args.Supercritical || !Exists(ent.Comp.Colossus) || !TryComp<CosmicColossusComponent>(ent.Comp.Colossus, out var colossusComp))
            return;

        colossusComp.DeathTimer = _time.CurTime;
        colossusComp.Timed = true;
    }

    private void OnSupercritical(Entity<CosmicEffigyComponent> ent, ref AnomalySupercriticalEvent args)
    {
        if (!Exists(ent.Comp.Colossus)
            || !TryComp<CosmicColossusComponent>(ent.Comp.Colossus, out var colossusComp)
            || !_mind.TryGetMind(ent.Comp.Colossus.Value, out _, out var mind))
            return;

        var colossus = ent.Comp.Colossus.Value;

        if (TryComp<MeleeWeaponComponent>(colossus, out var weapon))
        {
            weapon.AttackRate *= ent.Comp.ColossusAttackRateMultiplier;
        }

        if (TryComp<CosmicCorruptingComponent>(colossus, out var corrupting))
        {
            corrupting.CorruptionSpeed *= ent.Comp.ColossusCorruptionSpeedMultiplier;
        }

        if (ent.Comp.ColossusHeal && TryComp<DamageableComponent>(colossus, out var damageable))
        {
            _damage.TryChangeDamage(ent.Comp.Colossus.Value, damageable.Damage / 2 * -1, true);
        }

        colossusComp.BonusDamage +=
            new DamageSpecifier(_proto.Index(colossusComp.BonusDamageType), ent.Comp.ColossusBonusDamage);

        var transform = Transform(ent.Comp.Colossus.Value);
        Spawn(colossusComp.BuffVfx, transform.Coordinates);

        if (colossusComp.CompletedEffigies == 0)
        {
            _audio.PlayStatic(colossusComp.ReawakenSfx,
                Filter.BroadcastMap(transform.MapID),
                transform.Coordinates,
                true);
        }
        else
        {
            _audio.PlayPvs(colossusComp.ReawakenSfx, ent);
        }

        colossusComp.CompletedEffigies += 1;

        if (colossusComp.CompletedEffigies >= colossusComp.MaxEffigies)
        {
            colossusComp.Timed = false;
            _popup.PopupEntity(Loc.GetString("colossus-buff-final-popup"), colossus, PopupType.Large);
            return;
        }

        _popup.PopupEntity(Loc.GetString("colossus-buff-popup"), colossus, PopupType.Large);

        var objIndex = mind.Objectives.FindIndex(HasComp<CosmicEffigyConditionComponent>);
        if (objIndex == -1 ||
            !TryComp<CosmicEffigyConditionComponent>(mind.Objectives[objIndex], out var conditionComp))
        {
            Log.Error($"Failed to find effigy objective on {ToPrettyString(colossus)}!");
            return;
        }

        var objective = mind.Objectives[objIndex];
        if (_cultObjective.RandomizeEffigyTarget(objective, conditionComp, setDescription: true) is not {} nextTarget)
        {
            Log.Error("Failed to randomize effigy objective location!");
            return;
        }

        if (mind.UserId is { } userId)
        {
            _chat.DispatchServerMessage(_player.GetSessionById(userId), Loc.GetString("colossus-next-target", ("location", nextTarget)));
        }

        _codeCondition.SetCompleted(objective, false);

        colossusComp.EffigyPlaceActionEntity = _actions.AddAction(colossus, colossusComp.EffigyPlaceAction);
        colossusComp.DeathTimer = _time.CurTime + colossusComp.DeathWaitEffigy;
        colossusComp.Timed = true;
    }

    private void OnColossusEffigy(Entity<CosmicColossusComponent> ent, ref EventCosmicColossusEffigy args)
    {
        if (!VerifyPlacement(ent, out var pos))
            return;

        _actions.RemoveAction(ent.Owner, ent.Comp.EffigyPlaceActionEntity);
        _codeCondition.SetCompleted(ent.Owner, ent.Comp.EffigyObjective);
        var effigy = Spawn(ent.Comp.EffigyPrototype, pos);
        ent.Comp.Timed = false;

        if (!TryComp<CosmicEffigyComponent>(effigy, out var effigyComp))
        {
            Log.Error("Colossus tried to place Effigy prototype missing CosmicEffigyComponent!");
            return;
        }

        effigyComp.Colossus = ent.Owner;
    }

    private bool VerifyPlacement(Entity<CosmicColossusComponent> ent, out EntityCoordinates outPos)
    {
        // MAKE SURE WE'RE STANDING ON A GRID
        var xform = Transform(ent);
        outPos = new EntityCoordinates();

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
        {
            _popup.PopupEntity(Loc.GetString("ghost-role-colossus-effigy-error-grid"), ent, ent);
            return false;
        }

        var localTile = _map.GetTileRef(xform.GridUid.Value, grid, xform.Coordinates);
        var targetIndices = localTile.GridIndices + new Vector2i(0, 2);
        var pos = _map.ToCenterCoordinates(xform.GridUid.Value, targetIndices, grid);
        outPos = pos;
        var box = new Box2(pos.Position + new Vector2(-1.4f, -0.4f), pos.Position + new Vector2(1.4f, 0.4f));

        // CHECK IF IT'S BEING PLACED CHEESILY CLOSE TO SPACE
        var spaceDistance = 2;
        var worldPos = _transform.GetWorldPosition(xform);
        foreach (var tile in _map.GetTilesIntersecting(xform.GridUid.Value, grid, new Circle(worldPos, spaceDistance)))
        {
            if (_turf.IsSpace(tile))
            {
                _popup.PopupEntity(Loc.GetString("ghost-role-colossus-effigy-error-space", ("DISTANCE", spaceDistance)), ent, ent);
                return false;
            }
        }

        // CHECK FOR ENTITY AND ENVIRONMENTAL INTERSECTIONS
        if (_lookup.AnyLocalEntitiesIntersecting(xform.GridUid.Value, box, LookupFlags.Dynamic | LookupFlags.Static, ent))
        {
            _popup.PopupEntity(Loc.GetString("ghost-role-colossus-effigy-error-intersection"), ent, ent);
            return false;
        }

        // IF THE OBJECTIVE OR LOCATION IS MISSING, PLACE IT ANYWHERE
        if (!_mind.TryGetObjectiveComp<CosmicEffigyConditionComponent>(ent, out var obj) || obj.EffigyTarget == null)
            return true;

        var targetXform = Transform(obj.EffigyTarget.Value);
        if (xform.MapID != targetXform.MapID || (_transform.GetWorldPosition(xform) - _transform.GetWorldPosition(targetXform)).LengthSquared() > 15 * 15)
        {
            if (TryComp<WarpPointComponent>(obj.EffigyTarget, out var warp) && warp.Location is not null)
                _popup.PopupEntity(Loc.GetString("ghost-role-colossus-effigy-error-location", ("LOCATION", warp.Location)), ent, ent);
            return false;
        }

        return true;
    }
}
