namespace Content.Shared.FootPrint;

[RegisterComponent]
public sealed partial class PuddleFootPrintsComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public float SizeRatio = 0.2f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float OffPercent = 80f;
}
