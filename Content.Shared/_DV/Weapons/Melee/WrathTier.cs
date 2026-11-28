using Content.Shared.Damage;
using Robust.Shared.Serialization;
using static Content.Shared._DV.Weapons.Melee.WrathMeleeComponent;

namespace Content.Shared._DV.Weapons.Melee;

[DataDefinition, Serializable, NetSerializable]
public sealed partial class WrathMeleeTier
{
    [DataField]
    public int WrathRequired = 0;

    [DataField(required: true)]
    public DamageSpecifier Damage;

    [DataField]
    public float SpeedMultiplier = 1;

    [DataField]
    public float Reflect = 0;

    [DataField]
    public bool Activate = false;

    [DataField]
    public WrathMeleeTierVisual? Visual;
}
