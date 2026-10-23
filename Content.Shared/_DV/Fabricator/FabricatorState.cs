using Robust.Shared.Serialization;

namespace Content.Shared._DV.Fabricator;

[Serializable, NetSerializable]
public enum FabricatorVisualState
{
    Idle,
    Running,
}

public enum FabricatorVisualLayers : byte
{
    Base,
    BaseUnlit,
}

[Serializable, NetSerializable]
public enum FabricatorUiKey
{
    Key,
}
