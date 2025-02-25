using Content.Shared.EntityEffects;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.EntityEffects.EffectConditions;

public sealed partial class TypeDamage : EntityEffectCondition
{
    [DataField]
    public FixedPoint2 Max = FixedPoint2.MaxValue;

    [DataField]
    public FixedPoint2 Min = FixedPoint2.Zero;

    [DataField]
    public ProtoId<DamageGroupPrototype>? DamageGroup;

    [DataField]
    public ProtoId<DamageTypePrototype>? DamageType;

    public override bool Condition(EntityEffectBaseArgs args)
    {
        if (args.EntityManager.TryGetComponent(args.TargetEntity, out DamageableComponent? damage))
        {
            FixedPoint2 total;
            if (DamageGroup is { } group)
                total = damage.DamagePerGroup.GetValueOrDefault(group, FixedPoint2.Zero);
            else if (DamageType is { } kind)
                total = damage.Damage.DamageDict.GetValueOrDefault(kind, FixedPoint2.Zero);
            else
                total = damage.TotalDamage;

            if (total >= Min && total <= Max)
                return true;
        }

        return false;
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        if (DamageGroup is { } group)
        {
            var name = prototype.Index(group).LocalizedName;
            return Loc.GetString("reagent-effect-condition-guidebook-group-damage",
                ("max", Max == FixedPoint2.MaxValue ? (float) int.MaxValue : Max.Float()),
                ("min", Min.Float()),
                ("group", name));
        }
        else if (DamageType is { } kind)
        {
            var name = prototype.Index(kind).LocalizedName;
            return Loc.GetString("reagent-effect-condition-guidebook-type-damage",
                ("max", Max == FixedPoint2.MaxValue ? (float) int.MaxValue : Max.Float()),
                ("min", Min.Float()),
                ("type", name));
        }
        else
        {
            return Loc.GetString("reagent-effect-condition-guidebook-total-damage",
                ("max", Max == FixedPoint2.MaxValue ? (float) int.MaxValue : Max.Float()),
                ("min", Min.Float()));
        }
    }
}
