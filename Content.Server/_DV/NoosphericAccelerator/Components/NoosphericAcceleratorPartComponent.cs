namespace Content.Server._DV.NoosphericAccelerator.Components;

[RegisterComponent]
public sealed partial class NoosphericAcceleratorPartComponent : Component
{
    [ViewVariables]
    public EntityUid? Master;
}
