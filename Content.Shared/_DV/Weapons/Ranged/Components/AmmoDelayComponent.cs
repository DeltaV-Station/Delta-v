namespace Content.Shared._DV.Weapons.Ranged.Components;

[RegisterComponent]
public sealed partial class AmmoDelayComponent : Component
{
    [DataField]
    public float? Delay;

    [DataField]
    public float MinimumDelay = 1.0f;

    [DataField]
    public float MaximumDelay = 1.0f;
}
