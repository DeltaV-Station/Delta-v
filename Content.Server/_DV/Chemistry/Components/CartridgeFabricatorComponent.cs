using Content.Shared.Tag;

namespace Content.Server._DV.Chemistry.Components;

[RegisterComponent]
public sealed partial class CartridgeFabricatorComponent : Component
{
    [DataField]
    public string InputSolution = "drink";

    [DataField]
    public string OutputSolution = "cartridge";
}
