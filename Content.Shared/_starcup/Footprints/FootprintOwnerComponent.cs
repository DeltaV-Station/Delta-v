using Robust.Shared.Prototypes;

namespace Content.Shared._starcup.Footprints;

[RegisterComponent]
public sealed partial class FootprintOwnerComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public float Distance;

    [DataField]
    public int LastLayer = 0;

    [DataField]
    public string Solution = "print";

    [DataField]
    public EntProtoId FootprintEntityPrototype = "Footprint";

    [DataField]
    public ProtoId<FootprintPrototype> Footprint = "baseFootprint";

    [DataField]
    public ProtoId<FootprintPrototype> Bodyprint = "baseBodyprint";
}
