using System.Linq;
using Content.Shared.EntityConditions;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.EntityConditions.Conditions;

/// <summary>
///     Reagent effect condition that depends on if the entity has a given component(s), potentially on a body part.
/// </summary>
/// <inheritdoc cref="EntityConditionSystem{T, TCondition}"/>
public sealed partial class HasComponentsConditionSystem : EntityConditionSystem<MetaDataComponent, HasComponentCondition>
{
    [Dependency] private readonly EntityManager _ent = default!;

    protected override void Condition(Entity<MetaDataComponent> entity, ref EntityConditionEvent<HasComponentCondition> args)
    {
        var targetEntity = entity.Owner;

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

    public override string EntityConditionGuidebookText(IPrototypeManager prototype) => Loc.GetString(Explanation);
}
