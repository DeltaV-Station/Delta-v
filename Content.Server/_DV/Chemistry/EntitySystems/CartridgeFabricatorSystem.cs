using Content.Server._DV.Chemistry.Components;
using Content.Server.Popups;
using Content.Server.Power.EntitySystems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using YamlDotNet.Core.Tokens;

namespace Content.Server._DV.Chemistry.Systems;

public sealed class CartridgeFabricatorSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _container = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly TagSystem _tags = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private static readonly ProtoId<TagPrototype>[] BottleTags = {"Bottle"};

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CartridgeFabricatorComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteractUsing(Entity<CartridgeFabricatorComponent> entity, ref InteractUsingEvent args)
    {
        if (!this.IsPowered(entity.Owner, EntityManager))
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

        // Success, this is a bottle with an amount of _something_ in it.
        args.Handled = true;

        _popupSystem.PopupCursor("You made a thing!", args.User, PopupType.Medium);

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

        QueueDel(args.Used); // Ensure the old one is gone
    }
}
