using Content.Shared.Weapons.Melee.Events;
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

namespace Content.Shared.Stray.Plesen.PlesenCore;

#pragma warning disable IDE0055
#pragma warning disable CS0618
#pragma warning disable IDE1006
public abstract class SharedPlesenCoreSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] protected readonly SharedPlesenWallSystem _PWSystem = default!;
    [Dependency] protected readonly SharedPlesenFloorSystem _PFSystem = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly TileSystem _tile = default!;
    [Dependency] private readonly ITileDefinitionManager _tiledef = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;

    [ValidatePrototypeId<TagPrototype>]
    public const string wallTag = "Wall";

    [ValidatePrototypeId<TagPrototype>]
    public const string plesenTag = "Fungus";
    [ValidatePrototypeId<TagPrototype>]
    public const string windowTag = "Window";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlesenCoreComponent, ComponentInit>(OnInit);
        //SubscribeLocalEvent<PlesenCoreComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<PlesenCoreComponent, AttackedEvent>(OnAttacked);
        SubscribeLocalEvent<PlesenCoreComponent, AfterAutoHandleStateEvent>(OnAfterHandleState);
        //SubscribeLocalEvent<PlesenCoreComponent, MeleeThrowOnHitStartEvent>(OnAnomalyThrowStart);
        //SubscribeLocalEvent<PlesenCoreComponent, MeleeThrowOnHitEndEvent>(OnAnomalyThrowEnd);
    }
    private void OnInit(EntityUid uid, PlesenCoreComponent component, ref ComponentInit args){
        component.attachedCores.Add(component);
        component.realHealth = (component.fullyGroth?component.health:component.health/10);
        component.growAfter = Timing.CurTime+TimeSpan.FromSeconds(60);
        component.totalSpawnedCoresCount++;
    }
    public void OnAfterHandleState(EntityUid uid, PlesenCoreComponent component, AfterAutoHandleStateEvent args){
        UpdateVis(uid, component);
    }

    public void OnAttacked(EntityUid uid, PlesenCoreComponent component, AttackedEvent args){
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


    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var plesenQuery = EntityQueryEnumerator<PlesenCoreComponent>();
        while (plesenQuery.MoveNext(out var ent, out var plesen))
        {
            // if the stability is under the death threshold,
            // update it every second to start killing it slowly.
            //if (anomaly.Stability < anomaly.DecayThreshold)
            //{
            //    ChangeAnomalyHealth(ent, anomaly.HealthChangePerSecond * frameTime, anomaly);
            //}

            if (plesen.fullyGroth&&Timing.CurTime > plesen.updateTime)
            {
                UpdatePlesen(ent, plesen);
            }else if(!plesen.fullyGroth && Timing.CurTime > plesen.growAfter){
                SetGrouthState(ent, plesen, true);
            }
        }
    }

    public void SetGrouthState(EntityUid uid, PlesenCoreComponent component, bool toSet){
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

    protected virtual void UpdateVis(EntityUid uid, PlesenCoreComponent component){

    }

    public void UpdatePlesen(EntityUid uid, PlesenCoreComponent _c){
        _c.energy += 0.7f;
        _c.plesenStress = MathF.Max(_c.plesenStress-0.05f,0);


        _c.avarageEnergy = _c.energy;
        _c.avarageCoresHealth = _c.realHealth;
        _c.avarageHealth = _c.realHealth;

        for(int i = 0; i < _c.attachedCores.Count; i++){
            if(_c.attachedCores[i]==null){
                //_c.plesenStress+=1f;
                continue;
            }
            _c.avarageEnergy += _c.attachedCores[i].realHealth;
            _c.avarageHealth += _c.attachedCores[i].realHealth;
            _c.avarageCoresHealth += _c.attachedCores[i].realHealth;
        }
        for(int i = 0; i < _c.attachedWalls.Count; i++){
            if(_c.attachedWalls[i]==null){
                //_c.plesenStress+=0.1f;
                continue;
            }
            //_c.avarageEnergy += _c.attachedCores[i].energy;
            _c.avarageHealth += _c.attachedWalls[i].realHealth;
            if(Timing.CurTime> _c.attachedWalls[i].growAfter&&_c.attachedWalls[i].fullyGroth==false){
                _PWSystem.SetGrouthState(_c.attachedWalls[i].Owner, _c.attachedWalls[i], true);//_c.attachedWalls[i].prefullyGroth = true;
                //_PWSystem.UpdateVis(_c.attachedWalls[i].Owner, _c.attachedWalls[i]);
            }
            //_c.avarageCoresHealth += _c.health;
        }
        for(int i = 0; i < _c.attachedFloors.Count; i++){
            if(_c.attachedFloors[i]==null){
                //_c.plesenStress+=0.1f;
                continue;
            }
            //_c.avarageEnergy += _c.attachedCores[i].energy;
            _c.avarageHealth += _c.attachedFloors[i].realHealth;
            if(Timing.CurTime> _c.attachedFloors[i].growAfter&&_c.attachedFloors[i].fullyGroth==false){
                _PFSystem.SetGrouthState(_c.attachedFloors[i].Owner, _c.attachedFloors[i], true);//_c.attachedWalls[i].prefullyGroth = true;
                //_PFSystem.UpdateVis(_c.attachedFloors[i].Owner, _c.attachedFloors[i]);
            }
            //_c.avarageCoresHealth += _c.health;
        }
        for(int i = 0; i < _c.attachedCocones.Count; i++){
            //_c.avarageEnergy += _c.attachedCores[i].energy;
            //_c.avarageHealth += _c.attachedCocones[i].realHealth;
            if(_c.attachedCocones[i]==null){
                //_c.plesenStress+=0.1f;
                continue;
            }
            //_c.avarageEnergy += _c.attachedCores[i].energy;
            _c.avarageHealth += _c.attachedCocones[i].realHealth;
            //if(Timing.CurTime> _c.attachedCocones[i].growAfter&&_c.attachedCocones[i].fullyGroth==false){
            //    _PFSystem.SetGrouthState(_c.attachedCocones[i].Owner, _c.attachedCocones[i], true);//_c.attachedWalls[i].prefullyGroth = true;
            //    _PFSystem.UpdateVis(_c.attachedCocones[i].Owner, _c.attachedCocones[i]);
            //}
            //_c.avarageCoresHealth += _c.health;
        }
        _c.avarageHealth/=_c.attachedCores.Count+_c.attachedWalls.Count+_c.attachedFloors.Count+_c.attachedCocones.Count;
        _c.avarageEnergy/=_c.attachedCores.Count;
        _c.avarageCoresHealth/=_c.attachedCores.Count;


        if(_c.attachedFloors.Count<_c.totalSpawnedFloorsCount){
            _c.plesenStress+=0.2f;
            _c.totalSpawnedFloorsCount--;
        }
        if(_c.attachedWalls.Count<_c.totalSpawnedWallsCount){
            _c.plesenStress+=0.2f;
            _c.totalSpawnedWallsCount--;
        }
        if(_c.attachedCocones.Count<_c.totalSpawnedCoconesCount){
            _c.plesenStress+=0.5f;
            _c.totalSpawnedCoconesCount--;
        }
        if(_c.attachedCores.Count<_c.totalSpawnedCoresCount){
            _c.plesenStress+=10f;
            _c.totalSpawnedCoresCount--;
        }


        //if(_c.lastAvarageHealth - _c.avarageHealth>0){
        //    _c.plesenStress+=(_c.lastAvarageHealth - _c.avarageHealth)/10;
        //}

        _c.lastAvarageHealth = _c.avarageHealth;
        if(_c.energy>10 && _c.plesenStress<10){
            _c.updateTime = Timing.CurTime+TimeSpan.FromSeconds(_c.nextUpdateTime);
            EntityUid updt = uid;
            int randVal = _random.Next(0, 3);
            for(int i = 0; i < 1+((_c.attachedFloors.Count+_c.attachedWalls.Count+_c.attachedCores.Count*10)/10); i++){
                randVal = _random.Next(0, 3);
                if(randVal==0&&_c.attachedCores.Count>0){
                    updt = _random.Pick(_c.attachedCores).Owner;
                }else if(randVal==1&&_c.attachedWalls.Count>0){
                    updt = _random.Pick(_c.attachedWalls).Owner;
                }else if(_c.attachedFloors.Count>0){
                    updt = _random.Pick(_c.attachedFloors).Owner;
                }
                object[]? dat = GetObjectsOnGrid(updt, _c);
                if(dat!=null){
                    if((int)dat[3]==0){
                        //if(IsClientSide(uid)){
                        EntityUid spawned = EntityManager.SpawnEntity("FungusWall", _mapSystem.ToCenterCoordinates((TileRef)dat[1], (MapGridComponent)dat[2]));
                        if(TryComp(spawned, out PlesenWallComponent? pwc)){
                            //del((EntityUid)dat[0]);
                            _c.attachedWalls.Add(pwc);
                            //pwc.growAfter = Timing.CurTime+TimeSpan.FromSeconds(10);
                        }
                        _c.totalSpawnedWallsCount++;
                        _c.energy-=0.4f;
                        //}
                    }else if((int)dat[3]==1){
                        //_tile.ReplaceTile((TileRef)dat[1], (ContentTileDefinition)dat[4]);
                        //Spawn("FungusFloor", _mapSystem.ToCenterCoordinates((TileRef)dat[1], (MapGridComponent)dat[2]));
                        EntityUid spawned = EntityManager.SpawnEntity("FungusFloor", _mapSystem.ToCenterCoordinates((TileRef)dat[1], (MapGridComponent)dat[2]));
                        if(TryComp(spawned, out PlesenFloorComponent? pfc)){
                            _c.attachedFloors.Add(pfc);
                            //pfc.growAfter = Timing.CurTime+TimeSpan.FromSeconds(10);
                        }
                        _c.totalSpawnedFloorsCount++;
                        _c.energy-=0.3f;
                        //if(IsClientSide(uid)){
                        //    if(TryComp(Spawn("FungusWall", _mapSystem.ToCenterCoordinates((TileRef)dat[1], (MapGridComponent)dat[2])), out PlesenWallComponent? pwc)){
                        //        QueueDel((EntityUid)dat[0]);
                        //        _c.attachedWalls.Add(pwc);
                        //    }
                        //}
                    }else if((int)dat[3]==3&&_c.energy>35&&randVal!=0){
                        //_tile.ReplaceTile((TileRef)dat[1], (ContentTileDefinition)dat[4]);
                        //Spawn("FungusFloor", _mapSystem.ToCenterCoordinates((TileRef)dat[1], (MapGridComponent)dat[2]));
                        EntityUid spawned = EntityManager.SpawnEntity("PlesenCore", _mapSystem.ToCenterCoordinates((TileRef)dat[1], (MapGridComponent)dat[2]));
                        if(TryComp(spawned, out PlesenCoreComponent? pcc)){

                            SetCoreValues(spawned, pcc, _c.plesenStress, _c.attachedCores, _c.attachedWalls, _c.attachedFloors, _c.attachedCocones,
                            _c.totalSpawnedCoresCount, _c.totalSpawnedFloorsCount, _c.totalSpawnedWallsCount, _c.totalSpawnedCoconesCount);

                            _c.attachedCores.Add(pcc);
                            //pfc.SetCoreValues()
                            //pfc.growAfter = Timing.CurTime+TimeSpan.FromSeconds(10);
                        }
                        _c.totalSpawnedCoresCount++;
                        _c.energy-=15f;
                        //if(IsClientSide(uid)){
                        //    if(TryComp(Spawn("FungusWall", _mapSystem.ToCenterCoordinates((TileRef)dat[1], (MapGridComponent)dat[2])), out PlesenWallComponent? pwc)){
                        //        QueueDel((EntityUid)dat[0]);
                        //        _c.attachedWalls.Add(pwc);
                        //    }
                        //}
                    }
                }
            }
        }else if(_c.energy>1&&_c.plesenStress>=10){
            _c.updateTime = Timing.CurTime+TimeSpan.FromSeconds(_c.nextUpdateTime/10);
            EntityUid updt = uid;
            for(int i = 0; i < 10+((_c.attachedFloors.Count+_c.attachedWalls.Count+_c.attachedCores.Count*10)/10); i ++){

            if(_c.energy<1){
                break;
            }
            //plesenStress-=1;
            _c.plesenStress -= MathF.Max(_c.plesenStress-0.15f,0);
            int randVal = _random.Next(0, 3);
            if(randVal==0&&_c.attachedCores.Count>0){
                updt = _random.Pick(_c.attachedCores).Owner;
            }else if(randVal==1&&_c.attachedWalls.Count>0){
                updt = _random.Pick(_c.attachedWalls).Owner;
            }else if(_c.attachedFloors.Count>0){
                updt = _random.Pick(_c.attachedFloors).Owner;
            }
            object[]? dat = GetObjectsOnGrid(updt, _c);
            if(dat!=null){
                if((int)dat[3]==0){
                    //if(IsClientSide(uid)){
                    EntityUid spawned = EntityManager.SpawnEntity("FungusWall", _mapSystem.ToCenterCoordinates((TileRef)dat[1], (MapGridComponent)dat[2]));
                    if(TryComp(spawned, out PlesenWallComponent? pwc)){
                        //del((EntityUid)dat[0]);
                        _c.attachedWalls.Add(pwc);
                        //pwc.growAfter = Timing.CurTime+TimeSpan.FromSeconds(10);
                    }
                    _c.energy-=0.5f;
                    //}
                }else if((int)dat[3]==1){
                    //_tile.ReplaceTile((TileRef)dat[1], (ContentTileDefinition)dat[4]);
                    //Spawn("FungusFloor", _mapSystem.ToCenterCoordinates((TileRef)dat[1], (MapGridComponent)dat[2]));
                    EntityUid spawned = EntityManager.SpawnEntity("FungusFloor", _mapSystem.ToCenterCoordinates((TileRef)dat[1], (MapGridComponent)dat[2]));
                    if(TryComp(spawned, out PlesenFloorComponent? pfc)){
                        _c.attachedFloors.Add(pfc);
                        //pfc.growAfter = Timing.CurTime+TimeSpan.FromSeconds(10);
                    }
                    _c.energy-=0.3f;
                    //if(IsClientSide(uid)){
                    //    if(TryComp(Spawn("FungusWall", _mapSystem.ToCenterCoordinates((TileRef)dat[1], (MapGridComponent)dat[2])), out PlesenWallComponent? pwc)){
                    //        QueueDel((EntityUid)dat[0]);
                    //        _c.attachedWalls.Add(pwc);
                    //    }
                    //}
                }else if((int)dat[3]==3&&_c.energy>35&&randVal!=0){
                    //_tile.ReplaceTile((TileRef)dat[1], (ContentTileDefinition)dat[4]);
                    //Spawn("FungusFloor", _mapSystem.ToCenterCoordinates((TileRef)dat[1], (MapGridComponent)dat[2]));
                    EntityUid spawned = EntityManager.SpawnEntity("PlesenCore", _mapSystem.ToCenterCoordinates((TileRef)dat[1], (MapGridComponent)dat[2]));
                    if(TryComp(spawned, out PlesenCoreComponent? pcc)){

                        SetCoreValues(spawned, pcc, _c.plesenStress, _c.attachedCores, _c.attachedWalls, _c.attachedFloors, _c.attachedCocones,
                        _c.totalSpawnedCoresCount, _c.totalSpawnedFloorsCount, _c.totalSpawnedWallsCount, _c.totalSpawnedCoconesCount);

                        _c.attachedCores.Add(pcc);
                        //pfc.SetCoreValues()
                        //pfc.growAfter = Timing.CurTime+TimeSpan.FromSeconds(10);
                    }
                    _c.totalSpawnedCoresCount++;
                    _c.energy-=35f;
                    //if(IsClientSide(uid)){
                    //    if(TryComp(Spawn("FungusWall", _mapSystem.ToCenterCoordinates((TileRef)dat[1], (MapGridComponent)dat[2])), out PlesenWallComponent? pwc)){
                    //        QueueDel((EntityUid)dat[0]);
                    //        _c.attachedWalls.Add(pwc);
                    //    }
                    //}
                }
            }

            }
            //_c.attachedCores.Remove(null);
            //_c.attachedCores.RemoveAll( x => x==null);
            //_c.attachedWalls.RemoveAll( x => x==null);
            //_c.attachedFloors.RemoveAll( x => x==null);
        }
        //_c.Owner
    }

    public void SetCoreValues(EntityUid uid, PlesenCoreComponent _c, float plesenStress,
    List<PlesenCoreComponent> attachedCores,List<PlesenWallComponent> attachedWalls,
    List<PlesenFloorComponent> attachedFloors,List<PlesenCoconeComponent> attachedCocones,
    int totalSpawnedCoresCount,int totalSpawnedFloorsCount,int totalSpawnedWallsCount,int totalSpawnedCoconesCount ){
        _c.plesenStress = plesenStress;
        _c.attachedCores = attachedCores;
        _c.attachedWalls = attachedWalls;
        _c.attachedFloors = attachedFloors;
        _c.attachedCocones = attachedCocones;
        _c.totalSpawnedCoresCount = totalSpawnedCoresCount;
        _c.totalSpawnedFloorsCount = totalSpawnedFloorsCount;
        _c.totalSpawnedWallsCount = totalSpawnedWallsCount;
        _c.totalSpawnedCoconesCount = totalSpawnedCoconesCount;
    }

    public virtual void del(EntityUid toDel){

    }
    public object[]? GetObjectsOnGrid(EntityUid uid, PlesenCoreComponent _c){
        var xform = Transform(uid);
        if(xform==null){
            return null;
        }

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return null;

        var localpos = xform.Coordinates.Position;
        var physQuery = GetEntityQuery<PhysicsComponent>();
        var tilerefs = grid.GetLocalAnchoredEntities(
            new Box2(localpos + new Vector2(-1, -1), localpos + new Vector2(1, 1))).ToList();

        if (tilerefs == null || tilerefs.Count == 0)
            return null;

        //for(int i = 0; i < tilerefs.Count; i++){
        EntityUid? pass = null;
        int tryes = 0;
        EntityUid euid = _random.Pick(tilerefs);
        while(pass==null&&tryes<10){
            euid = _random.Pick(tilerefs);

            if (!physQuery.TryGetComponent(euid, out var body)){
                continue;
            }
            //if(TryComp(ent, out TagComponent? tag)){
            //    continue;
            //}
            if(_tag.HasTag(euid,plesenTag)){
                pass=null;
                break;
            }
            if (body.BodyType != BodyType.Static ||
                !body.Hard || !(_tag.HasTag(euid,wallTag)||_tag.HasTag(euid,windowTag))|| _tag.HasTag(euid,plesenTag) ||
                (body.CollisionLayer & (int) CollisionGroup.WallLayer) == 0)
            {
            pass=null;
            continue;
            }

            pass=euid;
                //break;

            tryes++;
        }
        //if(tryes>=10){
        //    return new object[]{uid, trf, grid, 2};
        //}
        tryes = 0;

        if(pass!=null){
            return new object[]{pass, grid.GetTileRef(Transform(euid).Coordinates.ToVector2i(_entityManager, _mapManager)), grid, 0};
        }
        var tilerefs2 = grid.GetLocalTilesIntersecting(
            new Box2(localpos + new Vector2(-1, -1), localpos + new Vector2(1, 1))).ToList();

        if (tilerefs2 == null || tilerefs2.Count == 0)
            return null;

        TileRef trf = _random.Pick(tilerefs2);

        if (trf.IsSpace()){
            return null;
        }

        bool pass2 = false;
        _tiledef.TryGetDefinition("FloorFungusBurned", out Robust.Shared.Map.ITileDefinition? tile );
        if(tile==null){
            return null;
        }else{
            while(pass2==false&&tryes<10){
                trf = _random.Pick(tilerefs2);
                foreach (var ent in grid.GetAnchoredEntities(trf.GridIndices)){
                    if(_tag.HasTag(ent,plesenTag)){
                        pass2=false;
                        break;
                    }
                    if(_tag.HasTag(ent,plesenTag)||_tag.HasTag(ent,wallTag)||_tag.HasTag(ent,windowTag)){
                        pass2=false;
                        continue;
                    }
                    pass2 = true;
                    //break;
                }
                tryes++;
            }
        }
        if(tryes>=10){
            return new object[]{uid, trf, grid, 3};
        }
        if(pass2){
            return new object[]{uid, trf, grid, 1, (ContentTileDefinition)tile};
        }
        //_tile.ReplaceTile(trf, (ContentTileDefinition)tile);

        //}
        return null;
    }
    //var pulseQuery = EntityQueryEnumerator<AnomalyPulsingComponent>();
    //while (pulseQuery.MoveNext(out var ent, out var pulse))
    //{
    //    if (Timing.CurTime > pulse.EndTime)
    //    {
    //        Appearance.SetData(ent, AnomalyVisuals.IsPulsing, false);
    //        RemComp(ent, pulse);
    //    }
    //}
    //
    //var supercriticalQuery = EntityQueryEnumerator<AnomalySupercriticalComponent, PlesenCoreComponent>();
    //while (supercriticalQuery.MoveNext(out var ent, out var super, out var anom))
    //{
    //    if (Timing.CurTime <= super.EndTime)
    //        continue;
    //    DoAnomalySupercriticalEvent(ent, anom);
    //    RemComp(ent, super);
    //}

}
