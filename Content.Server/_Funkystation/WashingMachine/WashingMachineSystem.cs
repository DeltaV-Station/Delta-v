using Content.Shared._Funkystation.WashingMachine;
using Content.Shared._Funkystation.Stains.Components;
using Content.Shared._Funkystation.Stains.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Destructible;
using Content.Shared.Storage.Components;
using Content.Server.Forensics;
using Content.Shared.Clothing.Components;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Linq;
using Content.Shared.Chemistry;
using Content.Shared.Damage.Systems;

namespace Content.Server._Funkystation.WashingMachine;

public sealed class WashingMachineSystem : SharedWashingMachineSystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solution = null!;
    [Dependency] private readonly SharedStainSystem _stains = null!;
    [Dependency] private readonly ForensicsSystem _forensics = null!;
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly IPrototypeManager _proto = null!;
    [Dependency] private readonly IRobustRandom _random = null!;
    [Dependency] private readonly ReactiveSystem _reactive = null!;

    private static readonly SoundSpecifier HitSound = new SoundCollectionSpecifier("MetalThud");

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WashingMachineComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<WashingMachineComponent, BreakageEventArgs>(OnBreak);
    }

    private void OnMapInit(Entity<WashingMachineComponent> ent, ref MapInitEvent args)
    {
        Appearance.SetData(ent.Owner, WashingMachineVisuals.State, ent.Comp.State);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<WashingMachineComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.State != WashingMachineState.Washing)
                continue;

            if (Timing.CurTime >= comp.WashFinishTime)
            {
                FinishWash(uid, comp);
                continue;
            }

            ProcessWashingHazards(uid, comp, frameTime);
        }
    }

    private void ProcessWashingHazards(EntityUid uid, WashingMachineComponent comp, float frameTime)
    {
        if (!TryComp<EntityStorageComponent>(uid, out var storage) || storage.Contents.ContainedEntities.Count == 0)
            return;

        var bluntProto = _proto.Index<DamageTypePrototype>("Blunt");
        var damage = new DamageSpecifier(bluntProto, comp.BluntDamagePerSecond * frameTime);

        var waterSpray = new Solution();
        waterSpray.AddReagent(comp.WaterSprayReagent, comp.WaterSprayAmount);

        var sprayWater = _random.Prob(comp.WaterSprayChance * frameTime);

        var hasHeavyItems = false;

        foreach (var item in storage.Contents.ContainedEntities)
        {
            _damageable.TryChangeDamage(item, damage, true);

            if (sprayWater)
                _reactive.DoEntityReaction(item, waterSpray, ReactionMethod.Touch);

            if (!hasHeavyItems && !HasComp<ClothingComponent>(item))
                hasHeavyItems = true;
        }

        if (hasHeavyItems)
        {
            if (_random.Prob(comp.ThumpSoundChance * frameTime))
                Audio.PlayPvs(HitSound, uid);

            comp.AccumulatedSelfDamage += comp.SelfDamagePerSecond * frameTime;
        }
    }

    protected override bool TryStartWash(Entity<WashingMachineComponent> ent, EntityUid user)
    {
        if (!base.TryStartWash(ent, user))
            return false;

        ent.Comp.AudioStream = Audio.PlayPvs(ent.Comp.WashLoopSound, ent.Owner)?.Entity;
        return true;
    }

    private void FinishWash(EntityUid uid, WashingMachineComponent comp)
    {
        comp.State = WashingMachineState.Idle;
        comp.WashFinishTime = null;
        comp.NextWashAllowed = Timing.CurTime + comp.Cooldown;

        Audio.Stop(comp.AudioStream);
        Audio.PlayPvs(comp.WashFinishedSound, uid);
        Appearance.SetData(uid, WashingMachineVisuals.State, WashingMachineState.Idle);

        HashSet<EntityUid> items = new();
        if (TryComp<EntityStorageComponent>(uid, out var storage))
        {
            items = storage.Contents.ContainedEntities.ToHashSet();
            foreach (var item in items)
            {
                if (TryComp<StainableComponent>(item, out var stain) && _solution.TryGetSolution(item, stain.SolutionName, out var sol))
                {
                    if (TryComp<ForensicsComponent>(uid, out var machineForensics))
                        machineForensics.DNAs.UnionWith(_forensics.GetSolutionsDNA(sol.Value.Comp.Solution));

                    _solution.RemoveAllSolution(sol.Value);
                    _stains.UpdateVisuals((item, stain));
                }
            }
        }

        var machineEv = new WashingMachineFinishedWashingEvent(items);
        RaiseLocalEvent(uid, machineEv);

        var itemEv = new WashingMachineWashedEvent(uid, items);
        foreach (var item in items)
        {
            RaiseLocalEvent(item, itemEv);
        }

        UpdateForensics((uid, comp), items);

        if (comp.AccumulatedSelfDamage > 0)
        {
            var bluntProto = _proto.Index<DamageTypePrototype>("Blunt");
            var selfDamage = new DamageSpecifier(bluntProto, comp.AccumulatedSelfDamage);
            _damageable.TryChangeDamage(uid, selfDamage, ignoreResistances: true);
            comp.AccumulatedSelfDamage = 0;
        }

        Dirty(uid, comp);
        Storage.OpenStorage(uid);
    }

    protected override void UpdateForensics(Entity<WashingMachineComponent> ent, HashSet<EntityUid> items)
    {
        if (!TryComp<ForensicsComponent>(ent.Owner, out var forensics))
            return;

        foreach (var item in items)
        {
            if (!TryComp<FiberComponent>(item, out var fiber))
                continue;

            var fiberText = fiber.FiberColor == null
                ? Loc.GetString("forensic-fibers", ("material", fiber.FiberMaterial))
                : Loc.GetString("forensic-fibers-colored", ("color", fiber.FiberColor), ("material", fiber.FiberMaterial));

            forensics.Fibers.Add(fiberText);
        }
    }

    private void OnBreak(Entity<WashingMachineComponent> ent, ref BreakageEventArgs args)
    {
        ent.Comp.State = WashingMachineState.Broken;
        ent.Comp.WashFinishTime = null;
        Audio.Stop(ent.Comp.AudioStream);
        Dirty(ent.Owner, ent.Comp);
        Appearance.SetData(ent.Owner, WashingMachineVisuals.State, WashingMachineState.Broken);
    }
}
