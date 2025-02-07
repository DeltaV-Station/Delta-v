using Content.Shared.Containers.ItemSlots;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Chemistry.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SolutionCartridgeReceiverComponent : Component
{
    public const string CartridgeSlotId = "cartridge-slot";

    [DataField("cartridgeSlot")]
    public ItemSlot CartridgeSlot = new();

    [DataField]
    public string HypospraySolution = "hypospray";

    [DataField]
    public string CartridgeSolution = "cartridge";

    [DataField]
    public FixedPoint2 MaximumVolume = FixedPoint2.New(30);

    [DataField]
    public SoundSpecifier InsertSound = new SoundPathSpecifier("/Audio/Weapons/Guns/MagIn/revolver_magin.ogg");
}
