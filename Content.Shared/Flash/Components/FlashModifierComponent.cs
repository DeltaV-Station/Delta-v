 namespace Content.Shared.Flash.Components;

[RegisterComponent]
public sealed partial class FlashModifierComponent : Component // NES14-Changes, Resomi
{
    [DataField]
    public float Modifier = 1f;
}
