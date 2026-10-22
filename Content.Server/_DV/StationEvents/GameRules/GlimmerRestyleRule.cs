using System.Linq;
using Content.Server._DV.StationEvents.Components;
using Content.Server.Body;
using Content.Server.Psionics;
using Content.Server.StationEvents.Events;
using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Events;
using Content.Shared._DV.Psionics.Systems;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Body;
using Content.Shared.GameTicking.Components;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Mobs.Components;
using Content.Shared.SSDIndicator;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._DV.StationEvents.Events;

public sealed class GlimmerRestyleRule : StationEventSystem<GlimmerRestyleRuleComponent>
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MobStateSystem _mob = default!;
    [Dependency] private readonly MarkingManager _markingManager = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedPsionicSystem _psionic = default!;
    [Dependency] private readonly VisualBodySystem _visualBodySystem = default!;

    protected override void Started(EntityUid uid, GlimmerRestyleRuleComponent comp, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, comp, gameRule, args);

        var query = EntityQueryEnumerator<HumanoidProfileComponent, MobStateComponent>();
        List<(EntityUid, HumanoidProfileComponent)> potentialTargets = new();

        while (query.MoveNext(out var entity, out var humanoid, out var mobState))
        {
            if (!_mob.IsAlive(entity, mobState) || !HasComp<PotentialPsionicComponent>(entity))
                continue;

            if (_psionic.CanBeTargeted(entity))
                potentialTargets.Add((entity, humanoid));
        }

        _random.Shuffle(potentialTargets);
        var targetsToRestyle = _random.Next(comp.MinimumTargets, comp.MaximumTargets);

        foreach (var (entity, humanoid) in potentialTargets)
        {
            // Due to this it never fired, since everyone has an SSDIndicatorComponent
            // if(HasComp<SSDIndicatorComponent>(entity))
            //     continue;

            if (targetsToRestyle-- <= 0)
                break;

            var changedHair = TryApplyRestyle((entity, humanoid), HumanoidVisualLayers.Hair,  comp.BaldChance);
            var changedFacialHair = TryApplyRestyle((entity, humanoid), HumanoidVisualLayers.FacialHair, comp.CleanShavenChance);
            if (changedHair || changedFacialHair)
                _popup.PopupEntity(Loc.GetString("glimmer-restyle-event"), entity, entity, PopupType.Medium);
            Dirty(entity, humanoid);
        }
    }

    private bool TryApplyRestyle(Entity<HumanoidProfileComponent> ent, HumanoidVisualLayers visualLayer, float noMarkingsChance)
    {
        var markings = new Dictionary<ProtoId<OrganCategoryPrototype>, Dictionary<HumanoidVisualLayers, List<Marking>>>();
        var markingsToApply = new Dictionary<HumanoidVisualLayers, List<Marking>>();
        var visualLayerSet = new HashSet<HumanoidVisualLayers>(){ visualLayer };
        ProtoId<OrganCategoryPrototype> category = "Head";

        if (!_visualBodySystem.TryGatherMarkingsData(ent.Owner,
                visualLayerSet,
                out _,
                out var markingData,
                out var prevApplied))
            return false;

        if (!markingData.TryGetValue(category, out var headMarkingData))
            return false;

        var newMarkingColor = new Color(_random.NextFloat(), _random.NextFloat(), _random.NextFloat());
        var availableMarkings =
            _markingManager.MarkingsByLayerAndGroupAndSex(visualLayer, headMarkingData.Group, ent.Comp.Sex);

        if (availableMarkings.Count == 0)
            return false;

        var newMarking = _random.Pick(availableMarkings.Values.ToList()).AsMarking().WithColor(newMarkingColor);

        var hadMarkings = prevApplied[category.ToString()].ContainsKey(visualLayer) &&
                          prevApplied[category.ToString()][visualLayer].Count > 0;
        var noMarkings = _random.Prob(noMarkingsChance);
        markings.Add(category, markingsToApply);
        markingsToApply.Add(visualLayer, noMarkings ? [] : [newMarking]);

        _visualBodySystem.ApplyMarkings(ent.Owner, markings);
        return hadMarkings || !noMarkings;
    }
}
