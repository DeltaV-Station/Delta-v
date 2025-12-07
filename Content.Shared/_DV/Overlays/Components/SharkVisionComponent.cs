using Content.Shared._Goobstation.Overlays;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Overlays.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SharkVisionComponent : SwitchableVisionOverlayComponent
{
    [DataField]
    public override EntProtoId? ToggleAction { get; set; } = "ActionSharkVisionPulse";

    [DataField]
    public override Color Color { get; set; } = Color.FromHex("#fc0800ff");

    public readonly ProtoId<ReagentPrototype>[] BloodPrototypes = [
        "Blood",
        "InsectBlood",
        "AmmoniaBlood",
        "CopperBlood",
        "ZombieBlood",
    ];
}
