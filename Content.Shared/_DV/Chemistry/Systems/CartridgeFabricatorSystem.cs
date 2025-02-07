using System.Linq;
using Content.Shared._DV.Chemistry.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Emag.Systems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Labels.Components;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Network;
using Content.Shared.Labels.EntitySystems;

namespace Content.Shared._DV.Chemistry.Systems;

public sealed class CartridgeFabricatorSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _container = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly TagSystem _tags = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedLabelSystem _labels = default!;
    private static readonly ProtoId<TagPrototype>[] BottleTags = ["Bottle"];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CartridgeFabricatorComponent, InteractUsingEvent>(OnInteractUsing);

        SubscribeLocalEvent<CartridgeFabricatorComponent, OnAttemptEmagEvent>(OnAttemptEmag);
        SubscribeLocalEvent<CartridgeFabricatorComponent, GotEmaggedEvent>(OnEmagged);
    }

    private bool ContainsDisallowedReagents(CartridgeFabricatorComponent fab, Solution solution)
    {
        foreach (ReagentQuantity reagent in solution.Contents)
        {
            if (!_prototype.TryIndex<ReagentPrototype>(reagent.Reagent.Prototype, out var prototype) ||
                prototype is null)
                continue; // TODO: This seems like an error case

            if (!fab.GroupWhitelist.Contains(prototype.Group) &&
                !fab.ReagentWhitelist.Contains(prototype.ID))
            {
                return true;
            }
        }

        return false;
    }

    private void OnInteractUsing(Entity<CartridgeFabricatorComponent> entity, ref InteractUsingEvent args)
    {
        if (args.Handled || !_power.IsPowered(entity.Owner))
            return;

        if (!TryComp(args.Used, out SolutionContainerManagerComponent? manager))
            return;

        if (!_tags.HasAnyTag(args.Used, BottleTags))
            return;

        if (!_container.TryGetSolution((args.Used, manager),
                entity.Comp.InputSolution,
                out var _,
                out var fromSolution))
            return;

        // This looks to be something we can handle
        args.Handled = true;

        if (fromSolution.Volume == 0)
        {
            _popup.PopupClient(Loc.GetString("cartridge-fabricator-empty-input"), args.User, PopupType.Medium);
            _audio.PlayPredicted(entity.Comp.FailureSound, entity.Owner, args.User);
            return;
        }

        // Emagging allows users to make cartridges out of anything
        if (!entity.Comp.Emagged &&
            ContainsDisallowedReagents(entity.Comp, fromSolution))
        {
            _popup.PopupClient(Loc.GetString("cartridge-fabricator-denied"), args.User, PopupType.Medium);
            _audio.PlayPredicted(entity.Comp.FailureSound, entity.Owner, args.User);
            return;
        }

        if (_net.IsServer)
        {
            var coords = Transform(entity.Owner).Coordinates;
            var cartridge = Spawn("BaseEmptyHypoCartridge", coords);

            var cartridgeManager = EnsureComp<SolutionContainerManagerComponent>(cartridge);
            if (!_container.TryGetSolution((cartridge, cartridgeManager),
                    entity.Comp.OutputSolution,
                    out var cartridgeSolutionEnt,
                    out var _))
                return; // Something very wrong here?

            if (!_container.TryAddSolution(cartridgeSolutionEnt.Value, fromSolution))
                return;

            if (TryComp<LabelComponent>(args.Used, out var bottleLabel))
            {
                // Propagate the label from the bottle onto the cartridge
                _labels.Label(cartridge, bottleLabel.CurrentLabel);
            }

            QueueDel(args.Used); // Ensure the bottle is gone
            _hands.TryPickupAnyHand(args.User, cartridge);
        }

        _popup.PopupClient(Loc.GetString("cartridge-fabricator-success", ("amount", fromSolution.Volume)),
            args.User,
            PopupType.Medium);
        _audio.PlayPredicted(entity.Comp.SuccessSound, entity.Owner, args.User);
    }

    private void OnAttemptEmag(Entity<CartridgeFabricatorComponent> entity, ref OnAttemptEmagEvent args)
    {
        if (entity.Comp.Emagged)
        {
            // No point in raising more local events when we're already emagged
            args.Handled = true;
            return;
        }
    }

    private void OnEmagged(Entity<CartridgeFabricatorComponent> entity, ref GotEmaggedEvent args)
    {
        entity.Comp.Emagged = true;
        args.Handled = true;
    }
}
