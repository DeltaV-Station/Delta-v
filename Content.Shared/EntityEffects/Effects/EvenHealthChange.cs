// DeltaV Start - Fix EvenHealing with Limbs.
using System.Linq;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Body.Systems;
// DeltaV End - Fix EvenHealing with Limbs.
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Content.Shared.Localizations;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.EntityEffects.Effects;

/// <summary>
/// Version of <see cref="HealthChange"/> that distributes the healing to groups
/// </summary>
public sealed partial class EvenHealthChange : EntityEffect
{
    /// <summary>
    /// Damage to heal, collected into entire damage groups.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<ProtoId<DamageGroupPrototype>, FixedPoint2> Damage = new();

    /// <summary>
    /// Should this effect scale the damage by the amount of chemical in the solution?
    /// Useful for touch reactions, like styptic powder or acid.
    /// Only usable if the EntityEffectBaseArgs is an EntityEffectReagentArgs.
    /// </summary>
    [DataField]
    public bool ScaleByQuantity;

    /// <summary>
    /// Should this effect ignore damage modifiers?
    /// </summary>
    [DataField]
    public bool IgnoreResistances = true;

    protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        var damages = new List<string>();
        var heals = false;
        var deals = false;

        var damagableSystem = entSys.GetEntitySystem<DamageableSystem>();
        var universalReagentDamageModifier = damagableSystem.UniversalReagentDamageModifier;
        var universalReagentHealModifier = damagableSystem.UniversalReagentHealModifier;

        foreach (var (group, amount) in Damage)
        {
            var groupProto = prototype.Index(group);

            var sign = FixedPoint2.Sign(amount);
            var mod = 1f;

            if (sign < 0)
            {
                heals = true;
                mod = universalReagentHealModifier;
            }
            else if (sign > 0)
            {
                deals = true;
                mod = universalReagentDamageModifier;
            }

            damages.Add(
                Loc.GetString("health-change-display",
                    ("kind", groupProto.LocalizedName),
                    ("amount", MathF.Abs(amount.Float() * mod)),
                    ("deltasign", sign)
                ));
        }

        var healsordeals = heals ? (deals ? "both" : "heals") : (deals ? "deals" : "none");
        return Loc.GetString("reagent-effect-guidebook-even-health-change",
            ("chance", Probability),
            ("changes", ContentLocalizationManager.FormatList(damages)),
            ("healsordeals", healsordeals));
    }

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (!args.EntityManager.TryGetComponent<DamageableComponent>(args.TargetEntity, out var damageable))
            return;

        var protoMan = IoCManager.Resolve<IPrototypeManager>();

        var scale = FixedPoint2.New(1);

        if (args is EntityEffectReagentArgs reagentArgs)
        {
            scale = ScaleByQuantity ? reagentArgs.Quantity * reagentArgs.Scale : reagentArgs.Scale;
        }

        var damagableSystem = args.EntityManager.System<DamageableSystem>();
        var dspec = GetDamageSpec(protoMan, damageable); // DeltaV - Fix EvenHealing with Limbs.

        damagableSystem.TryChangeDamage(
            args.TargetEntity,
            dspec * scale,
            IgnoreResistances,
            interruptsDoAfters: false,
            doPartDamage: false); // DeltaV - Fix EvenHealing with Limbs.

        // DeltaV Start - Fix EvenHealing with Limbs.
        var bodySystem = args.EntityManager.System<SharedBodySystem>();
        var bodyParts = SharedTargetingSystem.GetValidParts();
        foreach (var bodyPart in bodyParts)
        {
            var (targetType, targetSymmetry) =  bodySystem.ConvertTargetBodyPart(bodyPart);
            if (bodySystem.GetBodyChildrenOfType(args.TargetEntity, targetType, symmetry: targetSymmetry) is { } part)
            {
                if (!args.EntityManager.TryGetComponent<DamageableComponent>(part.FirstOrDefault().Id, out var damageablePart))
                    continue;
                dspec = GetDamageSpec(protoMan, damageablePart);

                if (dspec.GetTotal() == 0)
                    continue;

                damagableSystem.TryChangeDamage(
                    args.TargetEntity,
                    dspec * scale,
                    IgnoreResistances,
                    interruptsDoAfters: false,
                    targetPart: bodyPart,
                    onlyDamageParts: true,
                    canSever: false);
            }
        }
        // DeltaV End - Fix EvenHealing with Limbs.
    }

    private DamageSpecifier GetDamageSpec(IPrototypeManager protoMan, DamageableComponent damageable) // DeltaV - Fix EvenHealing with Limbs.
    {
        var damageSpecifier = new DamageSpecifier(); // DeltaV - Fix EvenHealing with Limbs.

        foreach (var (group, amount) in Damage)
        {
            var groupProto = protoMan.Index(group);
            var groupDamage = new Dictionary<string, FixedPoint2>();
            foreach (var damageId in groupProto.DamageTypes)
            {
                var damageAmount = damageable.Damage.DamageDict.GetValueOrDefault(damageId);
                if (damageAmount != FixedPoint2.Zero)
                    groupDamage.Add(damageId, damageAmount);
            }

            var sum = groupDamage.Values.Sum();
            foreach (var (damageId, damageAmount) in groupDamage)
            {
                var existing = damageSpecifier.DamageDict.GetOrNew(damageId); // DeltaV - Fix EvenHealing with Limbs.
                damageSpecifier.DamageDict[damageId] = existing + damageAmount / sum * amount; // DeltaV - Fix EvenHealing with Limbs.
            }
        }
        return damageSpecifier; // DeltaV - Fix EvenHealing with Limbs.
    }
}
