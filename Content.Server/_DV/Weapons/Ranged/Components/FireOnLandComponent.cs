namespace Content.Server._DV.Weapons.Ranged.Components;

[RegisterComponent]
public sealed partial class FireOnLandComponent : Component
{
    /// <summary>
    /// Chance to trigger.
    /// </summary>
    [DataField("Probability")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float Probability = 0.1f;
}
