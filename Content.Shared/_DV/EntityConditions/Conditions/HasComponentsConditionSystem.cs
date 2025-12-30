using System.Linq;
using Content.Shared.EntityEffects;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.EntityConditions;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Localization;

namespace Content.Shared._DV.EntityConditions.Conditions;

/// <summary>
///     Reagent effect condition that depends on if the entity has a given component(s), potentially on a body part.
/// </summary>
/// <inheritdoc cref="EntityConditionSystem{T, TCondition}"/>
public sealed partial class HasComponentsConditionSystem : EntityConditionSystem<MetaDataComponent, HasComponentCondition>
{
    [Dependency] private readonly EntityManager _ent = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;

    protected override void Condition(Entity<MetaDataComponent> entity, ref EntityConditionEvent<HasComponentCondition> args)
    {
        var targetEntity = args.Condition.BodyPart is { } bodyPart
                ? _body.GetBodyChildrenOfType(entity.Owner, bodyPart, symmetry: args.Condition.BodyPartSymmetry).Select(it => it.Id).FirstOrDefault()
                : entity.Owner;

        if (!targetEntity.IsValid())
        {
            args.Result = !args.Condition.ShouldHave;
            return;
        }

        var tested =
            args.Condition.ConsiderAll
                ? args.Condition.Components.Values.All(c => _ent.HasComponent(targetEntity, c.Component.GetType()))
                : args.Condition.Components.Values.Any(c => _ent.HasComponent(targetEntity, c.Component.GetType()));

        args.Result = tested ^ !args.Condition.ShouldHave;
    }
}

/// <inheritdoc cref="EntityCondition"/>
public sealed partial class HasComponentCondition : EntityConditionBase<HasComponentCondition>
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

    public override string EntityConditionGuidebookText(IPrototypeManager prototype) => Loc.GetString(Explanation);
}
