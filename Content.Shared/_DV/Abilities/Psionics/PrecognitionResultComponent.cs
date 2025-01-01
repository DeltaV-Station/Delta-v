namespace Content.Shared.Abilities.Psionics;

/// <summary>
/// Component added to game rules that lets it be shown with the precognition psionic power.
/// </summary>
[RegisterComponent]
public sealed partial class PrecognitionResultComponent : Component
{
    [DataField(required: true)]
    public LocId Message;

    [DataField]
    public float Weight = 1;
}
