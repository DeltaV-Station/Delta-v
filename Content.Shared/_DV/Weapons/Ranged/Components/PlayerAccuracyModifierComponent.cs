namespace Content.Shared._DV.Weapons.Ranged.Components;

[RegisterComponent]
public sealed partial class PlayerAccuracyModifierComponent : Component
{
    [DataField]
    public float SpreadModifier = 15f;

    [DataField]
    public float MaxSpreadAngle = 360f;
}
