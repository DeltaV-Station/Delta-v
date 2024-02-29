namespace Content.Server.DeltaV.Construction.Components;

[RegisterComponent]
public sealed partial class PreventDeconstructionComponent : Component
{
    [DataField]
    public bool RemoveWirePanel;
}
