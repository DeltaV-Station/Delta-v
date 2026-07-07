using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.CosmicCult.Components;

/// <summary>
/// The owning colossus's attack rate will be multiplied by
/// the given multiplier once its effigy goes supercritical.
/// Component meant to be applied to Cosmic Colossus.
/// </summary>
[RegisterComponent]
public sealed partial class MultiplyAttackRateOnSupercriticalComponent : Component
{
    [DataField]
    public float Multiplier = 1.1f;
}

/// <summary>
/// The owning colossus's corrupting speed will be multiplied by
/// the given multiplier once its effigy goes supercritical.
/// Component meant to be applied to Cosmic Colossus.
/// </summary>
[RegisterComponent]
public sealed partial class MultiplyCorruptingSpeedOnSupercriticalComponent : Component
{
    [DataField]
    public float Multiplier = 0.9f;
}

/// <summary>
/// The owning colossus will receive a flat bonus to
/// its melee attack damage once its effigy goes supercritical.
/// Component meant to be applied to Cosmic Colossus.
/// </summary>
[RegisterComponent]
public sealed partial class FlatAttackBonusOnSupercriticalComponent : Component
{
    [DataField]
    public FixedPoint2 BonusDamage = 10;

    [DataField]
    public ProtoId<DamageTypePrototype> BonusDamageType = "Blunt";
}

/// <summary>
/// The owning colossus will be healed for `-CurrentDamage * Abs(DamageFractionHealed)` damage
/// once its effigy goes supercritical.
/// Component meant to be applied to Cosmic Colossus.
/// </summary>
[RegisterComponent]
public sealed partial class HealOnSupercriticalComponent : Component
{
    [DataField]
    public float DamageFractionHealed = 1.0f;
}
