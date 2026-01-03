using Content.Server._DV.Objectives.Events;
using Content.Server.Actions;
using Content.Server.Antag;
using Content.Shared.Popups;
using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC;
using Content.Shared.Radio;
using Content.Shared.Radio.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.Laws.Components;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicFragmentationSystem : EntitySystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly CosmicCultSystem _cult = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;

    private ProtoId<RadioChannelPrototype> _cultRadio = "CosmicRadio";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AILawUpdatedEvent>(OnLawInserted);

        SubscribeLocalEvent<BorgChassisComponent, MalignFragmentationEvent>(OnFragmentBorg);
        SubscribeLocalEvent<SiliconLawUpdaterComponent, MalignFragmentationEvent>(OnFragmentAi);

        SubscribeLocalEvent<CosmicCultComponent, EventCosmicFragmentation>(OnCosmicFragmentation);
    }

    private void UnEmpower(Entity<CosmicCultComponent> ent)
    {
        var comp = ent.Comp;
        comp.CosmicEmpowered = false; // empowerment spent! Now we set all the values back to their default.
        comp.CosmicSiphonQuantity = CosmicCultComponent.DefaultCosmicSiphonQuantity;
        comp.CosmicGlareRange = CosmicCultComponent.DefaultCosmicGlareRange;
        comp.CosmicGlareDuration = CosmicCultComponent.DefaultCosmicGlareDuration;
        comp.CosmicGlareStun = CosmicCultComponent.DefaultCosmicGlareStun;
        comp.CosmicImpositionDuration = CosmicCultComponent.DefaultCosmicImpositionDuration;
        comp.CosmicBlankDuration = CosmicCultComponent.DefaultCosmicBlankDuration;
        comp.CosmicBlankDelay = CosmicCultComponent.DefaultCosmicBlankDelay;
        _actions.RemoveAction(ent.Owner, comp.CosmicFragmentationActionEntity);
        comp.CosmicFragmentationActionEntity = null;
    }

    private void OnCosmicFragmentation(Entity<CosmicCultComponent> ent, ref EventCosmicFragmentation args)
    {
        if (args.Handled || HasComp<ActiveNPCComponent>(args.Target) || _mobStateSystem.IsIncapacitated(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("cosmicability-generic-fail"), ent, ent);
            return;
        }
        var evt = new MalignFragmentationEvent(ent, args.Target);
        RaiseLocalEvent(args.Target, ref evt);
        if (evt.Cancelled) return;
        args.Handled = true;
        _popup.PopupEntity(Loc.GetString("cosmicability-fragmentation-success", ("user", ent), ("target", args.Target)), ent, PopupType.MediumCaution);
        _cult.MalignEcho(ent);
        UnEmpower(ent);
    }

    private void OnFragmentBorg(Entity<BorgChassisComponent> ent, ref MalignFragmentationEvent args)
    {
        if (!_mind.TryGetMind(args.Target, out var mindId, out var mind))
        {
            args.Cancelled = true;
            return;
        }
        var wisp = Spawn("CosmicChantryWisp", Transform(args.Target).Coordinates);
        var chantry = Spawn("CosmicBorgChantry", Transform(args.Target).Coordinates);
        EnsureComp<CosmicChantryComponent>(chantry, out var chantryComponent);
        chantryComponent.InternalVictim = wisp;
        chantryComponent.VictimBody = args.Target;
        _mind.TransferTo(mindId, wisp, mind: mind);

        var mins = chantryComponent.EventTime.Minutes;
        var secs = chantryComponent.EventTime.Seconds;
        _antag.SendBriefing(wisp, Loc.GetString("cosmiccult-silicon-chantry-briefing", ("minutesandseconds", $"{mins} minutes and {secs} seconds")), Color.FromHex("#4cabb3"), null);
    }

    private void OnFragmentAi(Entity<SiliconLawUpdaterComponent> ent, ref MalignFragmentationEvent args)
    {
        var lawboard = Spawn("CosmicCultLawBoard", Transform(args.Target).Coordinates);
        _container.TryGetContainer(args.Target, "circuit_holder", out var container);
        if (container == null)
            return;
        _container.EmptyContainer(container, true);
        _container.Insert(lawboard, container, Transform(args.Target), true);
    }

    private void OnLawInserted(AILawUpdatedEvent args)
    {
        if (!TryComp<IntrinsicRadioTransmitterComponent>(args.Target, out var radio) || !TryComp<ActiveRadioComponent>(args.Target, out var transmitter))
            return;
        if (args.Lawset.Id == "CosmicCultLaws")
        {
            radio.Channels.Add(_cultRadio);
            transmitter.Channels.Add(_cultRadio);
            _antag.SendBriefing(args.Target, Loc.GetString("cosmiccult-silicon-subverted-briefing"), Color.FromHex("#4cabb3"), null);
        }
        else
        {
            radio.Channels.Remove(_cultRadio);
            transmitter.Channels.Remove(_cultRadio);
        }
    }
}

[ByRefEvent]
public record struct MalignFragmentationEvent(Entity<CosmicCultComponent> User, EntityUid Target, bool Cancelled = false);
