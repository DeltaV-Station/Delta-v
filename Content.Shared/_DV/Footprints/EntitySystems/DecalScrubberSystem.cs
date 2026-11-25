using Content.Shared._DV.Footprints.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Decals;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._DV.Footprints.EntitySystems;

public sealed class DecalScrubberSystem : EntitySystem
{
    [Dependency] private readonly SharedDecalSystem _decal = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DecalScrubberComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<DecalScrubberComponent, DecalScrubberDoAfterEvent>(OnDoAfter);
    }

    private void OnDoAfter(Entity<DecalScrubberComponent> ent, ref DecalScrubberDoAfterEvent args)
    {
        if (args.Cancelled
            || ent.Comp.LastClick is not { } loc
            || _transform.GetGrid(loc) is not { } grid
            || !TryComp<DecalGridComponent>(grid, out var decalGrid))
            return;

        if (ent.Comp.CleaningSolutionName is { } solutionName &&
            _solution.TryGetSolution(ent.Owner, solutionName, out var solutionOpt) &&
            solutionOpt is {} solutionEnt)
        {
            var solution = solutionEnt.Comp.Solution;
            var toRemove = new ReagentQuantity(ent.Comp.CleaningReagent.Id, ent.Comp.CleaningReagentCost);
            if (solution.GetReagentQuantity(toRemove.Reagent) <= ent.Comp.CleaningReagentCost)
            {
                _popup.PopupPredicted(Loc.GetString("decal-scrubber-dry-popup", ("user", args.User)), ent.Owner, args.User);
                return;
            }

            _solution.RemoveReagent(solutionEnt, toRemove);
        }

        var decals = _decal.GetDecalsInRange(grid, loc.Position, ent.Comp.Radius, decal => decal.Cleanable);

        foreach (var (id, _) in decals)
        {
            _decal.RemoveDecal(loc.EntityId, id, decalGrid);
        }

        _audio.PlayPredicted(ent.Comp.ScrubSound, loc, args.User);
    }

    private void OnAfterInteract(Entity<DecalScrubberComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.CanReach || args.Handled || args.Target != null)
            return;

        ent.Comp.LastClick = args.ClickLocation;

        var ev = new DecalScrubberDoAfterEvent();
        var doArgs = new DoAfterArgs(_entityManager, args.User, ent.Comp.DoAfterLength, ev, ent.Owner)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnWeightlessMove = true,
            NeedHand = true,
        };

        if (_doAfter.TryStartDoAfter(doArgs))
            _popup.PopupPredicted(Loc.GetString("decal-scrubber-popup", ("user", args.User)), ent.Owner, args.User);
    }
}
