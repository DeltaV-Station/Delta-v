namespace Content.Server._DV.Footprints.Components;

/// <summary>
/// Component added to entities with PuddleComponent to enable entities leaving footprints after colliding with it.
/// System requires the following other components: AppearanceComponent, PuddleComponent, SolutionContainerManagerComponent
/// Colliding entity requires the FootPrintsComponent
/// </summary>
[RegisterComponent]
public sealed partial class PuddleFootPrintsComponent : Component
{
    [ViewVariables]
    public float SizeRatio = 0.2f;

    [ViewVariables]
    public float OffPercent = 80f;
}
