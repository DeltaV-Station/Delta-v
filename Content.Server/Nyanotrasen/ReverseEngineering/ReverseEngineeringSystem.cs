using Content.Server.Research.TechnologyDisk.Components;
using Content.Server.UserInterface;
using Content.Server.Power.Components;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.Research.Prototypes;
using Content.Shared.Research.TechnologyDisk.Components;
using Content.Shared.ReverseEngineering;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.ReverseEngineering;

public sealed class ReverseEngineeringSystem : SharedReverseEngineeringSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ReverseEngineeringMachineComponent, PowerChangedEvent>(OnPowerChanged);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ActiveReverseEngineeringMachineComponent, ReverseEngineeringMachineComponent>();
        while (query.MoveNext(out var uid, out var active, out var rev))
        {
            if (GetItem((uid, rev)) == null)
            {
                CancelProbe((uid, rev));
                continue;
            }

            if (Timing.CurTime < active.NextProbe)
                continue;

            ProbeItem((uid, rev, active));
        }
    }

    private void OnPowerChanged(Entity<ReverseEngineeringMachineComponent> ent, ref PowerChangedEvent args)
    {
        if (!args.Powered)
            CancelProbe(ent);
    }

    private ReverseEngineeringTickResult Roll(Entity<ReverseEngineeringMachineComponent> ent)
    {
        var (uid, comp) = ent;
        var roll = comp.ScanBonus;
        for (var i = 0; i < 3; i++)
            roll += _random.Next(1, 6);

        if (!comp.SafetyOn)
            roll += comp.DangerBonus;

        roll -= GetDifficulty(ent);

        return roll switch
        {
            // never let it be destroyed with safety on
            <= 9 => comp.SafetyOn
                ? ReverseEngineeringTickResult.Stagnation
                : ReverseEngineeringTickResult.Destruction,
            <= 10 => ReverseEngineeringTickResult.Stagnation,
            <= 12 => ReverseEngineeringTickResult.SuccessMinor,
            <= 15 => ReverseEngineeringTickResult.SuccessAverage,
            <= 17 => ReverseEngineeringTickResult.SuccessMajor,
            _ => ReverseEngineeringTickResult.InstantSuccess
        };
    }

    private void ProbeItem(Entity<ReverseEngineeringMachineComponent, ActiveReverseEngineeringMachineComponent> ent)
    {
        var (uid, comp, active) = ent;
        if (GetItem((uid, comp)) is not {} item)
            return;

        if (!TryComp<ReverseEngineeringComponent>(item, out var rev))
        {
            Log.Error($"We somehow scanned a {ToPrettyString(item):item} for reverse engineering...");
            return;
        }

        var result = Roll((uid, comp));
        if (result == ReverseEngineeringTickResult.Destruction)
        {
            _popup.PopupEntity(Loc.GetString("reverse-engineering-popup-failure", ("machine", uid)), uid, PopupType.MediumCaution);

            Eject((uid, comp));
            Del(item);

            foreach (var sound in comp.FailSounds)
                Audio.PlayPvs(sound, uid);

            UpdateUI((uid, comp));
            CancelProbe((uid, comp));
            return;
        }

        comp.LastResult = result;
        Dirty(uid, comp);

        var bonus = result switch
        {
            ReverseEngineeringTickResult.Stagnation => 1,
            ReverseEngineeringTickResult.SuccessMinor => 10,
            ReverseEngineeringTickResult.SuccessAverage => 25,
            ReverseEngineeringTickResult.SuccessMajor => 40,
            ReverseEngineeringTickResult.InstantSuccess => 100
        };

        rev.Progress = Math.Clamp(rev.Progress + bonus, 0, 100);
        Dirty(item, rev);

        if (rev.Progress < 100)
        {
            if (ent.Comp1.AutoScan)
                StartProbing(ent);
            else
                CancelProbe((uid, comp));
        }
        else
        {
            rev.Progress = 0;
            CreateDisk((uid, comp), rev.Recipes);
            Eject((uid, comp));
            Audio.PlayPvs(comp.SuccessSound, uid);
            if (rev.NewItem is {} proto)
            {
                Spawn(proto, Transform(uid).Coordinates);
                Del(item);
            }
        }

        UpdateUI((uid, comp));
    }

    private void CreateDisk(Entity<ReverseEngineeringMachineComponent> ent, List<ProtoId<LatheRecipePrototype>> recipes)
    {
        var uid = Spawn(ent.Comp.DiskPrototype, Transform(ent).Coordinates);
        var disk = Comp<TechnologyDiskComponent>(uid);
        disk.Recipes = new();
        foreach (var id in recipes)
            disk.Recipes.Add(id);
    }
}
