using Content.Shared._Funkystation.WashingMachine;
using Content.Shared._Funkystation.Stains.Components;
using Content.Shared._Funkystation.Stains.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Storage.Components;
using Content.Server.Forensics;
using Content.Shared.Clothing.Components;
using Robust.Shared.Audio;
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
    [Dependency] private readonly IRobustRandom _random = null!;
    [Dependency] private readonly ReactiveSystem _reactive = null!;

    private static readonly SoundSpecifier HitSound = new SoundCollectionSpecifier("MetalThud");

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WashingMachineComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<WashingMachineComponent> machine, ref MapInitEvent args)
    {
        Appearance.SetData(machine.Owner, WashingMachineVisuals.State, machine.Comp.State);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<WashingMachineComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.State != WashingMachineState.Washing || comp.NextWashingStep > Timing.CurTime)
                continue;

            if (Timing.CurTime >= comp.WashFinishTime)
            {
                FinishWash((uid, comp));
                continue;
            }

            comp.NextWashingStep = Timing.CurTime + comp.WashingStepCooldown;
            ProcessWashingHazards((uid, comp));
        }
    }

    private void ProcessWashingHazards(Entity<WashingMachineComponent> machine)
    {
        if (!TryComp<EntityStorageComponent>(machine, out var storage) || storage.Contents.ContainedEntities.Count == 0)
            return;

        var reagentSpray = new Solution();
        reagentSpray.AddReagent(machine.Comp.SprayReagent, machine.Comp.ReagentSprayAmount);

        // We store them in a hashset as gibbing will modify the collection and cause an error.
        var entitiesToWash = storage.Contents.ContainedEntities.ToHashSet();
        var doSpray = _random.Prob(machine.Comp.ReagentSprayChance);
        var hasHeavyItems = false;

        foreach (var item in entitiesToWash)
        {
            _damageable.TryChangeDamage(item, machine.Comp.EntityBluntDamage, true);

            if (doSpray)
                _reactive.DoEntityReaction(item, reagentSpray, ReactionMethod.Touch);

            if (!hasHeavyItems && !HasComp<ClothingComponent>(item))
                hasHeavyItems = true;
        }

        if (hasHeavyItems && _random.Prob(machine.Comp.ThumpSoundChance))
            Audio.PlayPvs(HitSound, machine);
    }

    protected override bool TryStartWash(Entity<WashingMachineComponent> machine, EntityUid user)
    {
        if (!base.TryStartWash(machine, user))
            return false;

        machine.Comp.AudioStream = Audio.PlayPvs(machine.Comp.WashLoopSound, machine.Owner)?.Entity;
        return true;
    }

    private void FinishWash(Entity<WashingMachineComponent> machine)
    {
        machine.Comp.State = WashingMachineState.Idle;
        machine.Comp.WashFinishTime = null;
        machine.Comp.NextWashAllowed = Timing.CurTime + machine.Comp.Cooldown;

        Audio.Stop(machine.Comp.AudioStream);
        Audio.PlayPvs(machine.Comp.WashFinishedSound, machine);
        Appearance.SetData(machine, WashingMachineVisuals.State, WashingMachineState.Idle);

        var hasHeavyItems = false;
        HashSet<EntityUid> items = new();
        if (TryComp<EntityStorageComponent>(machine, out var storage))
        {
            items = storage.Contents.ContainedEntities.ToHashSet();
            foreach (var item in items)
            {
                if (!hasHeavyItems && !HasComp<ClothingComponent>(item))
                    hasHeavyItems = true;

                if (!TryComp<StainableComponent>(item, out var stain)
                    || !_solution.TryGetSolution(item, stain.SolutionName, out var sol))
                    continue;

                if (TryComp<ForensicsComponent>(machine, out var machineForensics))
                    machineForensics.DNAs.UnionWith(_forensics.GetSolutionsDNA(sol.Value.Comp.Solution));

                _solution.RemoveAllSolution(sol.Value);
                _stains.UpdateVisuals((item, stain));
            }
        }

        var machineEv = new WashingMachineFinishedWashingEvent(items);
        RaiseLocalEvent(machine, machineEv);

        var itemEv = new WashingMachineWashedEvent(machine, items);
        foreach (var item in items)
        {
            RaiseLocalEvent(item, itemEv);
        }

        UpdateForensics((machine, machine), items);

        if (hasHeavyItems && machine.Comp.SelfDamage.AnyPositive())
        {
            _damageable.TryChangeDamage(machine.Owner, machine.Comp.SelfDamage * machine.Comp.WashTime.TotalSeconds, ignoreResistances: true);
        }

        Storage.OpenStorage(machine);
        Dirty(machine);
    }

    private void UpdateForensics(Entity<WashingMachineComponent> machine, HashSet<EntityUid> items)
    {
        if (!TryComp<ForensicsComponent>(machine.Owner, out var forensics))
            return;

        foreach (var item in items)
        {
            if (!TryComp<FiberComponent>(item, out var fiber))
                continue;

            var fiberLocale = string.IsNullOrEmpty(fiber.FiberColor)
                ? Loc.GetString("forensic-fibers", ("material", fiber.FiberMaterial))
                : Loc.GetString("forensic-fibers-colored", ("color", fiber.FiberColor), ("material", fiber.FiberMaterial));

            forensics.Fibers.Add(fiberLocale + " ; " + fiber.Fiberprint);
        }
    }
}
