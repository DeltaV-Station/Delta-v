using Robust.Shared.Map;
using Robust.Shared.Serialization;
using Content.Shared.DoAfter;

namespace Content.Shared.Arachne
{
    [Serializable, NetSerializable]
    public sealed partial class ArachneWebDoAfterEvent : DoAfterEvent
    {
        [DataField("coords", required: true)]
        public EntityCoordinates Coords = default!;

        private ArachneWebDoAfterEvent()
        {
        }

        public ArachneWebDoAfterEvent(EntityCoordinates coords)
        {
            Coords = coords;
        }

        public override DoAfterEvent Clone() => this;
    }

    [Serializable, NetSerializable]
    public sealed partial class ArachneCocoonDoAfterEvent : SimpleDoAfterEvent
    {
    }
}
