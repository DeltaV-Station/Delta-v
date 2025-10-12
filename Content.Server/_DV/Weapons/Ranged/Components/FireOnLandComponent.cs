namespace Content.Server._DV.Weapons.Ranged.Components;

[RegisterComponent]
public sealed partial class FireOnLandComponent : Component
{
    /// <summary>
    /// Chance to trigger.
    /// </summary>
    [DataField]
    public float Probability = 1.0f;
}
