namespace Content.Server._DV.Footprints.Components;

[RegisterComponent]
public sealed partial class PuddleFootPrintsComponent : Component
{
    [ViewVariables()]
    public float SizeRatio = 0.2f;

    [ViewVariables()]
    public float OffPercent = 80f;
}
