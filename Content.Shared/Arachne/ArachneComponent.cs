using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Arachne
{
    [RegisterComponent]
    public sealed partial class ArachneComponent : Component
    {
        [DataField("webDelay")]
        public float WebDelay = 5f;

        [DataField("cocoonDelay")]
        public float CocoonDelay = 12f;

        [DataField("cocoonKnockdownMultiplier")]
        public float CocoonKnockdownMultiplier = 0.5f;

        /// <summary>
        /// Blood reagent required to web up a mob.
        /// </summary>

        [DataField("webBloodReagent")]
        public string WebBloodReagent = "Blood";

        [DataField("webActionId",
        customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? WebActionId = "SpinWeb";

        [DataField("dispelActionEntity")]
        public EntityUid? WebActionEntity;
    }
}
