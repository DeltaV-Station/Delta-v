using Robust.Shared.Serialization;

namespace Content.Shared._DV.CosmicCult;

[Serializable, NetSerializable]
public sealed partial class CosmicSiphonIndicatorEvent : EntityEventArgs
{
    public NetEntity Target;

    public CosmicSiphonIndicatorEvent(NetEntity target)
    {
        Target = target;
    }

    public CosmicSiphonIndicatorEvent() : this(new())
    {
    }
}
