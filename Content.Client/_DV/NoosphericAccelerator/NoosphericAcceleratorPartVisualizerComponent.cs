using Content.Shared._DV.NoospericAccelerator.Components;

namespace Content.Client._DV.NoosphericAccelerator;

[RegisterComponent]
[Access(typeof(NoosphericAcceleratorPartVisualizerSystem))]
public sealed partial class NoosphericAcceleratorPartVisualsComponent : Component
{
    [DataField("stateBase", required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public string StateBase = default!;

    [DataField("stateSuffixes")]
    [ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<NoosphericAcceleratorVisualState, string> StatesSuffixes = new()
    {
        {NoosphericAcceleratorVisualState.Powered, "p"},
        {NoosphericAcceleratorVisualState.Level0, "p0"},
        {NoosphericAcceleratorVisualState.Level1, "p1"},
        {NoosphericAcceleratorVisualState.Level2, "p2"},
        {NoosphericAcceleratorVisualState.Level3, "p3"},
    };
}
