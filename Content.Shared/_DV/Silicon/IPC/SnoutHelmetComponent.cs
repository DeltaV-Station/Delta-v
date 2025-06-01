namespace Content.Shared._DV.Silicon.IPC;

[RegisterComponent]
public sealed partial class SnoutHelmetComponent : Component
{
    [DataField]
    public bool? EnableAlternateHelmet = false;

    [DataField(readOnly: true, required: true)]
    public string ReplacementRace;
}
