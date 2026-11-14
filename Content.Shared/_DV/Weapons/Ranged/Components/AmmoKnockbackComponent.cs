namespace Content.Shared._DV.Weapons.Ranged.Components;

[RegisterComponent]
public sealed partial class AmmoKnockbackComponent : Component
{
    [DataField]
    public float KnockbackStrength = 25.0f;

    [DataField]
    public float KnockdownTime = 1.0f;

    /// <summary>
    /// Should you get knocked down causing you to drop items and be stunned
    /// </summary>
    [DataField]
    public bool Knockdown = false;
}
