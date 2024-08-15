using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Damage;
using Content.Shared.Weapons.Melee;
using Content.Shared.Stray.Plesen.PlesenCore;
using Content.Shared.Stray.Plesen.PlesenWall;
using Content.Shared.Stray.Plesen.PlesenFloor;
using Content.Shared.Stray.Plesen.PlesenCocone;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Damage;
using Content.Shared.Tag;
using Robust.Shared.Timing;
using Content.Shared.Physics;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.GameObjects;
using Content.Shared.Coordinates;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Content.Shared.Maps;
using Robust.Shared.Prototypes;
using System.Linq;
using System.Numerics;
using Robust.Shared.Random;

namespace Content.Shared.Stray.Plesen.PlesenFloor;

public abstract class SharedPlesenFloorSystem : EntitySystem
{
    //[Dependency] private readonly MeleeWeaponSystem _meleeWeapon = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] private readonly TileSystem _tile = default!;
    //[Dependency] private readonly IRobustRandom _random = default!;
    //[Dependency] private readonly DamageableSystem _damageable = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlesenFloorComponent, ComponentInit>(OnInit);
        //SubscribeLocalEvent<PlesenCoreComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<PlesenFloorComponent, AttackedEvent>(OnAttacked);
        SubscribeLocalEvent<PlesenFloorComponent, AfterAutoHandleStateEvent>(OnAfterHandleState);
        //SubscribeLocalEvent<PlesenCoreComponent, MeleeThrowOnHitStartEvent>(OnAnomalyThrowStart);
        //SubscribeLocalEvent<PlesenCoreComponent, MeleeThrowOnHitEndEvent>(OnAnomalyThrowEnd);
    }

    public void OnInit(EntityUid uid, PlesenFloorComponent component, ComponentInit args){
        component.realHealth = (component.fullyGroth?component.health:component.health/10);
        component.growAfter = Timing.CurTime+TimeSpan.FromSeconds(10);
    }
    public void OnAfterHandleState(EntityUid uid, PlesenFloorComponent component, AfterAutoHandleStateEvent args){
        UpdateVis(uid, component);
    }

    public void SetGrouthState(EntityUid uid, PlesenFloorComponent component, bool toSet){
        component.fullyGroth = true;
        component.realHealth = (component.fullyGroth?component.health:component.health/10);
        //List<EntityUid>? tres = GetSpawningPoints(uid);
        UpdateVis(uid, component);
        //if(tres!=null){
        //    foreach(EntityUid t in tres){
        //        del(t);
        //    }
        //}
    }
    protected virtual void UpdateVis(EntityUid uid, PlesenFloorComponent component){

    }

    public void OnAttacked(EntityUid uid, PlesenFloorComponent component, AttackedEvent args){
        //for(int i = 0; )
        if(!TryComp(uid, out DamageableComponent? dmc)){
            return;
        }
        //float addDam = 0;
        //for(int i = 0; i < (args.BonusDamage+dmc.Damage).DamageDict.Count; i++){
        //    addDam += (args.BonusDamage+dmc.Damage).DamageDict.Ge;
        //}
        component.realHealth = (component.fullyGroth?component.health:component.health/10)-dmc.Damage.GetTotal().Value/100;
        if(component.realHealth <= 0){
            //if(IsClientSide(uid)){
                //if(TryComp(Spawn("FungusWall", _mapSystem.ToCenterCoordinates((TileRef)dat[1], (MapGridComponent)dat[2])), out PlesenWallComponent? pwc)){
            del(uid);
            //}
        }
        //args.BonusDamage.DamageDict.Values
        //_meleeWeapon.GetDamage(args.Used, args.User).Empty
    }
    public virtual void del(EntityUid toDel){

    }
    /*
    public List<EntityUid>? GetSpawningPoints(EntityUid uid)
    {
        var xform = Transform(uid);

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return null;

        //var amount = (int) (MathHelper.Lerp(settings.MinAmount, settings.MaxAmount, severity * stability * powerModifier) + 0.5f);

        var localpos = xform.Coordinates.Position;
        var tilerefs = grid.GetLocalTilesIntersecting(
            new Box2(localpos + new Vector2(0.1f, 0.1f), localpos + new Vector2(0.1f, 0.1f))).ToList();

        if (tilerefs.Count == 0)
            return null;

        var physQuery = GetEntityQuery<PhysicsComponent>();
        var resultList = new List<EntityUid>();
        //while (resultList.Count < amount)
        //{
        for(int i = 0; i < tilerefs.Count; i++){
            //var tileref = _random.Pick(tilerefs);
        //var distance = MathF.Sqrt(MathF.Pow(tileref.X - xform.LocalPosition.X, 2) + MathF.Pow(tileref.Y - xform.LocalPosition.Y, 2));
        //cut outer & inner circle
       //if (distance > settings.MaxRange || distance < settings.MinRange)
       //{
       //    tilerefs.Remove(tileref);
       //    continue;
       //}
        //if (!settings.CanSpawnOnEntities)
        //{
            var valid = true;
            foreach (var ent in grid.GetAnchoredEntities(tilerefs[i].GridIndices))
            {
                if (!physQuery.TryGetComponent(ent, out var body))
                    continue;
                if (body.BodyType != BodyType.Static ||
                    !body.Hard ||
                    (body.CollisionLayer & (int) CollisionGroup.Impassable) == 0)
                    continue;
                resultList.Add(ent);
                //valid = false;
                break;
            }
            //if (!valid)
            //{
            //    //tilerefs.Remove(tilerefs[i]);
            //    continue;
            //}

        }
        //}

        //}
        return resultList;
    }
    */
}
