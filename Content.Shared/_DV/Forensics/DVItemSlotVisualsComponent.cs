using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared._DV.Forensics;

[RegisterComponent, NetworkedComponent]
public sealed partial class DVItemSlotVisualsComponent : Component
{
    [DataField(required: true)]
    public string ItemSlot;

    [DataField(required: true)]
    public SpriteSpecifier FilledSprite;

    [DataField(required: true)]
    public SpriteSpecifier UnfilledSprite;
}
