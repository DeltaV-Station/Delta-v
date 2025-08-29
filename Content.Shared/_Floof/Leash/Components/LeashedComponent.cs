namespace Content.Shared.Floofstation.Leash.Components;

[RegisterComponent]
public sealed partial class LeashedComponent : Component
{
    public const string VisualsContainerName = "leashed-visuals";

    [DataField]
    public string? JointId = null;

    [NonSerialized]
    public EntityUid? Puller = null, Anchor = null;
}
