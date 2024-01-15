using Content.Shared.DeltaV.Harpy;

namespace Content.Server.DeltaV.Harpy;

[RegisterComponent]
public sealed partial class HarpyVisualsComponent : SharedHarpyVisualsComponent
{
    [DataField]
    public bool Worn { get; set; }
}
