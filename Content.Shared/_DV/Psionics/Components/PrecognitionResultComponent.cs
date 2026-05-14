namespace Content.Shared.Abilities.Psionics;

/// <summary>
/// Component added to game rules that lets it be shown with the precognition psionic power.
/// </summary>
[RegisterComponent]
public sealed partial class PrecognitionResultComponent : Component
{
    /// <summary>
    /// The message that will warn the psionic about the coming event.
    /// </summary>
    [DataField(required: true)]
    public LocId Message;

    [DataField]
    public float Weight = 1;
}
