using Robust.Shared.Serialization;

namespace Content.Shared._DV.CosmicCult;

[Serializable, NetSerializable]
public sealed class CosmicSiphonIndicatorEvent(NetEntity target) : EntityEventArgs
{
    public NetEntity Target = target;

    public CosmicSiphonIndicatorEvent() : this(new NetEntity())
    {
    }
}
