using Content.Server._DV.CosmicCult.Components;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.CosmicCult.Abilities.Colossus;

public sealed class CosmicColossusBuffsSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MultiplyAttackRateOnSupercriticalComponent, CosmicColossusEffigySupercriticalEvent>(HandleAttackRate);
        SubscribeLocalEvent<FlatAttackBonusOnSupercriticalComponent, CosmicColossusEffigySupercriticalEvent>(HandleDamageBonus);
        SubscribeLocalEvent<MultiplyCorruptingSpeedOnSupercriticalComponent, CosmicColossusEffigySupercriticalEvent>(HandleCorruptingSpeed);
        SubscribeLocalEvent<HealOnSupercriticalComponent, CosmicColossusEffigySupercriticalEvent>(HandleHeal);
    }

    private void HandleAttackRate(Entity<MultiplyAttackRateOnSupercriticalComponent> ent,
        ref CosmicColossusEffigySupercriticalEvent args)
    {
        if (TryComp<MeleeWeaponComponent>(ent, out var weapon))
        {
            weapon.AttackRate *= ent.Comp.Multiplier;
        }
    }

    private void HandleDamageBonus(Entity<FlatAttackBonusOnSupercriticalComponent> ent,
        ref CosmicColossusEffigySupercriticalEvent args)
    {
        if (!TryComp<CosmicColossusComponent>(ent, out var colossusComp))
            return;

        colossusComp.BonusDamage +=
            new DamageSpecifier(_proto.Index(ent.Comp.BonusDamageType), ent.Comp.BonusDamage);
    }

    private void HandleCorruptingSpeed(Entity<MultiplyCorruptingSpeedOnSupercriticalComponent> ent,
        ref CosmicColossusEffigySupercriticalEvent args)
    {
        if (TryComp<CosmicCorruptingComponent>(ent, out var corrupting))
        {
            corrupting.CorruptionSpeed *= ent.Comp.Multiplier;
        }
    }

    private void HandleHeal(Entity<HealOnSupercriticalComponent> ent, ref CosmicColossusEffigySupercriticalEvent args)
    {
        if (TryComp<DamageableComponent>(ent, out var damageable))
        {
            _damage.TryChangeDamage(ent.Owner,
                -damageable.Damage * Math.Abs(ent.Comp.DamageFractionHealed), // just in case someone sets a negative value in YML
                true);
        }
    }
}
