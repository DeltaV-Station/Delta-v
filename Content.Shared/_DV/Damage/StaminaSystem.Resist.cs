using Content.Shared.Armor;
using Content.Shared.Damage.Components;
using Content.Shared.Inventory;
using Robust.Shared.Audio;

namespace Content.Shared.Damage.Systems;

public sealed partial class StaminaSystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;

    /// <summary>
    /// Gets the combined stamina protection coefficients from all armor worn by an entity
    /// </summary>
    private float GetMeleeCoefficient(EntityUid target)
    {
        var coefficient = 1.0f;

        if (!_inventory.TryGetSlots(target, out var slots))
            return coefficient;

        foreach (var slot in slots)
        {
            if (!_inventory.TryGetSlotEntity(target, slot.Name, out var equipped))
                continue;

            if (TryComp<ArmorComponent>(equipped, out var armor))
            {
                coefficient *= armor.StaminaMeleeDamageCoefficient;
            }
        }

        return coefficient;
    }

    /// <summary>
    /// Gets the combined stamina protection coefficients from all armor worn by an entity
    /// </summary>
    private float GetProjectileCoefficient(EntityUid target)
    {
        var coefficient = 1.0f;

        if (!_inventory.TryGetSlots(target, out var slots))
            return coefficient;

        foreach (var slot in slots)
        {
            if (!_inventory.TryGetSlotEntity(target, slot.Name, out var equipped))
                continue;

            if (TryComp<ArmorComponent>(equipped, out var armor))
            {
                coefficient *= armor.StaminaDamageCoefficient;
            }
        }

        return coefficient;
    }

    /// <summary>
    /// Applies stamina damage from melee attacks with armor resistance calculations
    /// </summary>
    public void TakeMeleeStaminaDamage(EntityUid target,
        float damage,
        StaminaComponent? stamina = null,
        EntityUid? source = null,
        EntityUid? with = null,
        bool visual = true,
        SoundSpecifier? sound = null)
    {
        if (!Resolve(target, ref stamina))
            return;

        var coefficient = GetMeleeCoefficient(target);
        var finalDamage = damage * coefficient;

        TakeStaminaDamage(target, finalDamage, stamina, source, with, visual, sound);
    }

    /// <summary>
    /// Applies stamina damage from projectiles with armor resistance calculations
    /// </summary>
    public void TakeProjectileStaminaDamage(EntityUid target,
        float damage,
        StaminaComponent? stamina = null,
        EntityUid? source = null,
        EntityUid? with = null,
        bool visual = true,
        SoundSpecifier? sound = null)
    {
        if (!Resolve(target, ref stamina))
            return;

        var coefficient = GetProjectileCoefficient(target);
        var finalDamage = damage * coefficient;

        TakeStaminaDamage(target, finalDamage, stamina, source, with, visual, sound);
    }
}
