using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Cocoon
{
    [Serializable, NetSerializable]
    public sealed partial class CocoonDoAfterEvent : SimpleDoAfterEvent
    {
    }

    [Serializable, NetSerializable]
    public sealed partial class UnCocoonDoAfterEvent : SimpleDoAfterEvent
    {
    }
}
