using System.Linq;
using Content.Shared.EntityEffects;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Prototypes;
using Robust.Shared.Localization;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;

namespace Content.Shared._DV.EntityEffects.EffectConditions;

/// <summary>
///     Reagent effect condition that depends on if the entity has a given component(s), potentially on a body part
/// </summary>
public sealed partial class HasComponent : EntityEffectCondition
{
    /// <summary>
    ///     The set of components that this condition cares about
    /// </summary>
    [DataField(required: true)]
    public ComponentRegistry Components;

    /// <summary>
    ///     Whether or not the given components should be present
    /// </summary>
    [DataField]
    public bool ShouldHave = true;

    /// <summary>
    ///     Whether the check is an existential or universal check
    /// </summary>
    [DataField]
    public bool ConsiderAll;

    /// <summary>
    ///     The explanation displayed in the guidebook for this condition
    /// </summary>
    [DataField(required: true)]
    public LocId Explanation;

    /// <summary>
    ///     The body part of the entity to test for the components
    /// </summary>
    [DataField]
    public BodyPartType? BodyPart;

    /// <summary>
    ///     The side of the entity's body to test for the components
    /// </summary>
    [DataField]
    public BodyPartSymmetry? BodyPartSymmetry;

    public override bool Condition(EntityEffectBaseArgs args)
    {
        var _body = args.EntityManager.System<SharedBodySystem>();
        var entity =
            BodyPart is {} bodyPart
                ? _body.GetBodyChildrenOfType(args.TargetEntity, bodyPart, symmetry: BodyPartSymmetry).Select(it => it.Id).FirstOrDefault()
                : args.TargetEntity;

        if (!entity.IsValid())
        {
            return !ShouldHave;
        }

        var tested =
            ConsiderAll
                ? Components.Values.All(c => args.EntityManager.HasComponent(entity, c.Component.GetType()))
                : Components.Values.Any(c => args.EntityManager.HasComponent(entity, c.Component.GetType()));

        return tested ^ !ShouldHave;
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        return Loc.GetString(Explanation);
    }
}
