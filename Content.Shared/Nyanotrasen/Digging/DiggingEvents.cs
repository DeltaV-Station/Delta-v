using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared.Nyanotrasen.Digging;


[Serializable, NetSerializable]
public sealed partial class EarthDiggingCompleteEvent : DoAfterEvent
{
    public NetCoordinates Coordinates { get; set; }
    public NetEntity Shovel;
    public override DoAfterEvent Clone()
    {
        return this;
    }
}

[Serializable, NetSerializable]
public sealed class EarthDiggingCancelledEvent : EntityEventArgs
{
    public NetEntity Shovel;
}
