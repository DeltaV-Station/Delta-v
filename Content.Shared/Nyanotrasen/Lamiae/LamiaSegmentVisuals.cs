using Robust.Shared.Serialization;

namespace Content.Shared.Nyanotrasen.Lamiae
{
    [Serializable, NetSerializable]
    public enum LamiaSegmentVisualLayers
    {
        Base,
    }

    [Serializable, NetSerializable]
    public enum SegmentBaseVisualLayer
    {
        Color,
    }

    [Serializable, NetSerializable]
    public enum SegmentArmorVisualLayer
    {
        Hardsuit,
        NoHardsuit,
    }
}
