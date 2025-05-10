using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Server.Radio.Components;
using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.DoAfter;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.NPC;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.StationAi;
using Robust.Shared.Containers;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicFragmentationSystem : EntitySystem
{
    [Dependency] private readonly CosmicCultSystem _cult = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicCultComponent, EventCosmicFragmentation>(OnCosmicFragmentation);
        SubscribeLocalEvent<CosmicCultComponent, EventCosmicChantryDoAfter>(OnCosmicChantryDoAfter);
        SubscribeLocalEvent<CosmicCultComponent, EventCosmicFragmentationDoAfter>(OnCosmicFragmentationDoAfter);
    }

    private void UnEmpower(Entity<CosmicCultComponent> ent)
    {
        ent.Comp.CosmicEmpowered = false; // empowerment spent! Now we set all the values back to their default.
        ent.Comp.CosmicSiphonQuantity = 1;
        ent.Comp.CosmicGlareRange = 8;
        ent.Comp.CosmicGlareDuration = TimeSpan.FromSeconds(5);
        ent.Comp.CosmicGlareStun = TimeSpan.FromSeconds(0);
        ent.Comp.CosmicImpositionDuration = TimeSpan.FromSeconds(5.8);
        ent.Comp.CosmicBlankDuration = TimeSpan.FromSeconds(22);
        ent.Comp.CosmicBlankDelay = TimeSpan.FromSeconds(0.6);
    }

    private void OnCosmicFragmentation(Entity<CosmicCultComponent> ent, ref EventCosmicFragmentation args)
    {
        if (args.Handled || HasComp<ActiveNPCComponent>(args.Target) || TryComp<MobStateComponent>(args.Target, out var state) && state.CurrentState != MobState.Alive)
        {
            _popup.PopupEntity(Loc.GetString("cosmicability-generic-fail"), ent, ent);
            return;
        }
        if (HasComp<EmagSiliconLawComponent>(args.Target))
        {
            var doargs = new DoAfterArgs(EntityManager, ent, ent.Comp.CosmicSiphonDelay, new EventCosmicChantryDoAfter(), ent, args.Target)
            {
                DistanceThreshold = 2f,
                Hidden = false,
                BreakOnHandChange = true,
                BreakOnDamage = true,
                BreakOnMove = true,
                BreakOnDropItem = true,
            };
            args.Handled = true;
            _doAfter.TryStartDoAfter(doargs);
        }
        else if (HasComp<SiliconLawUpdaterComponent>(args.Target))
        {
            var doargs = new DoAfterArgs(EntityManager, ent, ent.Comp.CosmicSiphonDelay, new EventCosmicFragmentationDoAfter(), ent, args.Target)
            {
                DistanceThreshold = 2f,
                Hidden = false,
                BreakOnHandChange = true,
                BreakOnDamage = true,
                BreakOnMove = true,
                BreakOnDropItem = true,
            };
            args.Handled = true;
            _doAfter.TryStartDoAfter(doargs);
        }

        _cult.MalignEcho(ent);
    }
    private void OnCosmicChantryDoAfter(Entity<CosmicCultComponent> ent, ref EventCosmicChantryDoAfter args)
    {
        if (args.Args.Target is not { } target)
            return;
        if (args.Cancelled || args.Handled)
            return;
        args.Handled = true;
        UnEmpower(ent);

        var chantry = Spawn(ent.Comp.BorgChantry, Transform(target).Coordinates);
        var polyVictim = _polymorph.PolymorphEntity(target, "CosmicFragmentationWisp");
        if (polyVictim == null) // this really shouldn't ever be null but whatever
            return;
        EnsureComp<CosmicChantryComponent>(chantry, out var chantryComponent);
        chantryComponent.PolyVictim = polyVictim.Value;
        chantryComponent.Victim = target;
    }
    private void OnCosmicFragmentationDoAfter(Entity<CosmicCultComponent> ent, ref EventCosmicFragmentationDoAfter args)
    {
        if (args.Args.Target is not { } target)
            return;
        if (args.Cancelled || args.Handled)
            return;
        args.Handled = true;
        UnEmpower(ent);

        var lawboard = Spawn(ent.Comp.MalignLawboard, Transform(target).Coordinates);
        _container.TryGetContainer(target, "circuit_holder", out var container);
        if (container == null)
            return;
        _container.EmptyContainer(container, true);
        _container.Insert(lawboard, container, Transform(target), true);

        var query = EntityQueryEnumerator<StationAiHeldComponent, StationAiVisionComponent>();
        while (query.MoveNext(out var targetAi, out _, out _))
        {
            var transmitter = EnsureComp<IntrinsicRadioTransmitterComponent>(targetAi);
            var radio = EnsureComp<ActiveRadioComponent>(targetAi);
            radio.Channels.Add("CosmicRadio");
            transmitter.Channels.Add("CosmicRadio");
        }
    }
}
