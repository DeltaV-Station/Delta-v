using Content.Shared.Damage;
using Robust.Shared.Serialization;
using static Content.Shared._DV.Weapons.Melee.AdrenalineMeleeComponent;

namespace Content.Shared._DV.Weapons.Melee;

[DataDefinition, Serializable, NetSerializable]
public sealed partial class AdrenalineMeleeTier
{
    [DataField]
    public int AdrenalineRequired = 0;

    [DataField(required: true)]
    public DamageSpecifier Damage;

    [DataField]
    public float SpeedMultiplier = 1;

    [DataField]
    public float Reflect = 0;

    [DataField]
    public bool Activate = false;

    [DataField]
    public AdrenalineMeleeTierVisual? Visual;
}