using Robust.Shared.Serialization;

namespace Content.Shared._DV.Silicons;

[Serializable, NetSerializable]
public enum BorgWireActionKey : byte
{
    SlavedKey,
    TransponderKey,
    ChipKey,
    CellKey,
}
