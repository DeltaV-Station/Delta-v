using Content.Shared.FixedPoint;
using Robust.Shared.Audio;

namespace Content.Shared._DV.Chemistry.Components;

[RegisterComponent]
public sealed partial class CartridgeFabricatorComponent : Component
{
    [DataField]
    public List<string> GroupWhitelist = [];

    [DataField]
    public List<string> ReagentWhitelist = [];

    [DataField]
    public string InputSolution = "drink";

    [DataField]
    public string OutputSolution = "cartridge";

    public bool Emagged = false;

    [DataField]
    public SoundSpecifier SuccessSound = new SoundPathSpecifier("/Audio/Machines/tray_eject.ogg");

    [DataField]
    public SoundSpecifier FailureSound = new SoundPathSpecifier("/Audio/Machines/custom_deny.ogg");
}
