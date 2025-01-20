using Content.Shared.DeviceLinking;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._DV.DeviceLinking.Components
{
    [RegisterComponent]
    public sealed partial class DeadMansSignallerComponent : Component

    {
        /// <summary>
        ///     The port that gets signaled when the switch turns on.
        /// </summary>
        [DataField("port", customTypeSerializer: typeof(PrototypeIdSerializer<SourcePortPrototype>))]
        public string Port = "Pressed";
    }
}
