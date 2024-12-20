namespace Content.Shared.Abilities.Psionics;

[RegisterComponent]
public sealed partial class PrecognitionResultComponent : Component
{
    [DataField]
    public string Message = default!;

    [DataField]
    public float Weight = 1;
}
