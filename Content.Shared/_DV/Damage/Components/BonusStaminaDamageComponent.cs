namespace Content.Shared._DV.Damage.Components;

[RegisterComponent]
public sealed partial class BonusStaminaDamageComponent : Component
{
    [DataField]
    public float Multiplier = 1.25f;
}
