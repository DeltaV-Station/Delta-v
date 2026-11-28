using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._ES.Weapons.Ranged.Attachments.Components;

/// <summary>
/// Used to hold data for guns which can have attachments added onto them.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(ESSharedGunAttachmentsSystem))]
public sealed partial class ESAttachableGunComponent : Component
{
    /// <summary>
    /// The slots that can have attachments added.
    /// </summary>
    [DataField]
    public List<ESGunAttachmentSlot> Slots = new();
}

[Serializable, NetSerializable]
[DataDefinition]
public partial record struct ESGunAttachmentSlot
{
    /// <summary>
    /// The name of this slot
    /// </summary>
    [DataField(required: true)]
    public LocId Name;

    /// <summary>
    /// Container associated with the slot where the item is stored.
    /// </summary>
    [DataField(required: true)]
    public string ContainerId;

    /// <summary>
    /// Whitelist used to define which particular attachments can be used with this slot.
    /// </summary>
    [DataField(required: true)]
    public EntityWhitelist Whitelist = new();
}

[Serializable, NetSerializable]
public sealed class ESAttachableGunModifySlotEvent(NetEntity gun, int slotIndex) : EntityEventArgs
{
    public NetEntity Gun = gun;
    public int SlotIndex = slotIndex;
}

[Serializable, NetSerializable]
public enum ESAttachableGunUiKey : byte
{
    Key,
}
