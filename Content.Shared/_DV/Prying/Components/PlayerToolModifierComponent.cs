namespace Content.Shared._DV.Prying.Components;

[RegisterComponent]
public sealed partial class PlayerToolModifierComponent : Component
{
    [DataField]
    public float PryTimeMultiplier = 1.0f;
}
