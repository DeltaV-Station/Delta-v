using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Abilities.Psionics
{
    [RegisterComponent]
    public sealed partial class PsionicInvisibilityPowerComponent : Component
    {
        [DataField("psionicInvisibilityActionId",
        customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? PsionicInvisibilityActionId = "ActionPsionicInvisibility";

        [DataField("psionicInvisibilityActionEntity")]
        public EntityUid? PsionicInvisibilityActionEntity;

        [DataField("InvisibilityFeedback")]
        public string InvisibilityFeedback = "invisibility-feedback";

        [DataField("UseTimer")]
        public float UseTimer = 30f;
    }
}
