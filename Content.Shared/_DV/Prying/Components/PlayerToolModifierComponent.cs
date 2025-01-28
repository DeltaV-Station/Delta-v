namespace Content.Shared._DV.Prying.Components;

[RegisterComponent]
public sealed partial class PlayerToolModifierComponent : Component
{
    [DataField]
    public float PryTimeModifier = 1.0f;
}
