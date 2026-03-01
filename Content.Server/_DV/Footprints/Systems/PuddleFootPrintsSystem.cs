using System.Linq;
using Content.Server._DV.Footprints.Components;
using Content.Shared._EE.Flight;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Fluids;
using Content.Shared.Fluids.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Footprints.Systems;

/// <summary>
/// Handles transferring puddle colors and reagents to entities with footprints when they step in puddles.
/// </summary>
public sealed class PuddleFootPrintsSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedFlightSystem _flight = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;

    private EntityQuery<FlightComponent> _flightQuery;

    private static readonly ProtoId<ReagentPrototype> WaterPrototype = "Water";

    public override void Initialize()
    {
        base.Initialize();

        _flightQuery = GetEntityQuery<FlightComponent>();

        SubscribeLocalEvent<PuddleFootPrintsComponent, EndCollideEvent>(OnEndCollide);
    }

    private void OnEndCollide(Entity<PuddleFootPrintsComponent> ent, ref EndCollideEvent args)
    {
        var tripper = args.OtherEntity;

        // Don't process if the tripper is flying
        if (_flightQuery.TryComp(ent, out var flight) && _flight.IsFlying((ent, flight)))
            return;

        // Only process entities that can leave footprints
        if (!TryComp<FootPrintsComponent>(tripper, out var footPrints))
            return;

        // Get puddle appearance and solution data
        if (!TryComp<AppearanceComponent>(ent, out var appearance))
            return;

        if (!TryComp<PuddleComponent>(ent, out var puddleComp))
            return;

        if (!TryComp<SolutionContainerManagerComponent>(ent, out var solutionManager))
            return;

        if (!_solutionContainer.ResolveSolution((ent, solutionManager),
                puddleComp.SolutionName,
            ref puddleComp.Solution,
                out var solution))
            return;

        // Calculate total solution quantity and water percentage
        var totalSolutionQuantity = solution.Contents.Sum(sol => (float)sol.Quantity);

        if (totalSolutionQuantity <= 0 || solution.Contents.Count <= 0)
            return;

        var waterQuantity = solution.Contents
            .Where(sol => sol.Reagent.Prototype == WaterPrototype)
            .Sum(sol => (float)sol.Quantity);

        var waterPercent = (waterQuantity / totalSolutionQuantity) * 100f;

        // If puddle is mostly water, don't transfer color
        if (waterPercent > ent.Comp.OffPercent)
            return;

        // Transfer color from puddle to footprints
        if (_appearance.TryGetData(ent, PuddleVisuals.SolutionColor, out var colorValue, appearance)
            && _appearance.TryGetData(ent, PuddleVisuals.CurrentVolume, out var volumeValue, appearance))
        {
            if (colorValue is Color color && volumeValue is float volume)
            {
                AddColor(color, volume * ent.Comp.SizeRatio, footPrints);
            }
        }

        // Remove small amount of reagent from puddle
        _solutionContainer.RemoveEachReagent(puddleComp.Solution.Value, footPrints.AmountToTransfer);
    }

    private void AddColor(Color color, float quantity, FootPrintsComponent component)
    {
        // If no color yet, use the puddle's color directly
        if (component.ColorQuantity == 0f)
        {
            component.PrintsColor = color;
        }
        else
        {
            // Interpolate between current color and new color
            component.PrintsColor = Color.InterpolateBetween(
                component.PrintsColor,
                color,
                component.ColorInterpolationFactor);
        }

        component.ColorQuantity += quantity;
    }
}
