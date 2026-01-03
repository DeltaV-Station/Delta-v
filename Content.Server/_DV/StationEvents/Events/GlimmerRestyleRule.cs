using System.Linq;
using Content.Server._DV.StationEvents.Components;
using Content.Server.Psionics;
using Content.Server.StationEvents.Events;
using Content.Shared.Abilities.Psionics;
using Content.Shared.GameTicking.Components;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Mobs.Components;
using Content.Shared.SSDIndicator;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Random;

namespace Content.Server._DV.StationEvents.Events;

public sealed class GlimmerRestyleRule : StationEventSystem<GlimmerRestyleRuleComponent>
{
    [Dependency] private readonly MobStateSystem _mob = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MarkingManager _markingManager = default!;

    protected override void Started(EntityUid uid, GlimmerRestyleRuleComponent comp, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, comp, gameRule, args);

        var query = EntityQueryEnumerator<HumanoidAppearanceComponent, MobStateComponent>();
        List<(EntityUid, HumanoidAppearanceComponent)> potentialTargets = new();

        while (query.MoveNext(out var entity, out var humanoid, out var mobState))
        {
            if (!_mob.IsAlive(entity, mobState) || HasComp<PsionicInsulationComponent>(entity) || !HasComp<PotentialPsionicComponent>(entity))
                continue;
            potentialTargets.Add((entity, humanoid));
        }

        _random.Shuffle(potentialTargets);
        var targetsToRestyle = _random.Next(comp.MinimumTargets, comp.MaximumTargets);

        foreach (var (entity, humanoid) in potentialTargets)
        {
            if(HasComp<SSDIndicatorComponent>(entity))
                continue;

            if (targetsToRestyle-- <= 0)
                break;

            var changedHair = TryApplyRestyle((entity, humanoid), MarkingCategories.Hair, comp.BaldChance);
            var changedFacialHair = TryApplyRestyle((entity, humanoid), MarkingCategories.FacialHair, comp.CleanShavenChance);
            if (changedHair || changedFacialHair)
                _popup.PopupEntity(Loc.GetString("glimmer-restyle-event"), entity, entity, PopupType.Medium);
            Dirty(entity, humanoid);
        }
    }

    private bool TryApplyRestyle(Entity<HumanoidAppearanceComponent> ent, MarkingCategories category, float noMarkingsChance)
    {
        var newMarkingColor = new Color(_random.NextFloat(), _random.NextFloat(), _random.NextFloat());
        var availableMarkings = _markingManager.MarkingsByCategoryAndSpecies(category, ent.Comp.Species);
        if (availableMarkings.Count == 0)
            return false;

        var hadCategoryBefore = ent.Comp.MarkingSet.TryGetCategory(category, out _);
        ent.Comp.MarkingSet.RemoveCategory(category);
        if (_random.Prob(noMarkingsChance))
            return hadCategoryBefore; //Do not show the popup if you go from no markings to no markings.

        var newMarking = _random.Pick(availableMarkings.Values.ToList()).AsMarking();
        newMarking.SetColor(newMarkingColor);

        ent.Comp.MarkingSet.AddCategory(category);
        ent.Comp.MarkingSet.AddFront(category, newMarking);
        return true;
    }
}
