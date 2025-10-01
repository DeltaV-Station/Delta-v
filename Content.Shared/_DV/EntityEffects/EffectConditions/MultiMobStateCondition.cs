using Content.Shared.EntityEffects;
using Content.Shared.Localizations;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.EntityEffects.EffectConditions;

/// <summary>
/// This works like the upstream MobStateCondition, but it accepts a list of states instead of just one.
/// Helps with de-cluttering the guidebook for stuff that does one thing for dead mobs and a different one
/// for critical and alive mobs.
/// </summary>
public sealed partial class MultiMobStateCondition : EntityEffectCondition
{
    [DataField(required: true)]
    public List<MobState> States = new();

    public override bool Condition(EntityEffectBaseArgs args)
    {
        if (args.EntityManager.TryGetComponent(args.TargetEntity, out MobStateComponent? mobState))
        {
            return States.Contains(mobState.CurrentState);
        }

        return false;
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        var stateStrings = new List<string>(States.Count);
        foreach (var state in States)
        {
            stateStrings.Add(state.ToString().ToLower());
        }
        var formattedStates = ContentLocalizationManager.FormatListToOr(stateStrings);

        return Loc.GetString("reagent-effect-condition-guidebook-mob-state-condition", ("state", formattedStates));
    }
}
