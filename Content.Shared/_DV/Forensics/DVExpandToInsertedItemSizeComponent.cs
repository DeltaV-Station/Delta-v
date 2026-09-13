using Content.Shared.Item;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Forensics;

[RegisterComponent, NetworkedComponent]
public sealed partial class DVExpandToInsertedItemSizeComponent : Component
{
    [DataField(required: true)]
    public string ItemSlot;

    [DataField(required: true)]
    public ProtoId<ItemSizePrototype> EmptySize;
}
