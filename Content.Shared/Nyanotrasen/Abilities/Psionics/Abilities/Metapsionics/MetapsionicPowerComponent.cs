using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Abilities.Psionics
{
    [RegisterComponent]
    public sealed partial class MetapsionicPowerComponent : Component
    {
        [DataField("doAfter")]
        public DoAfterId? DoAfter;

        [DataField("useDelay")]
        public float UseDelay = 8f;
        [DataField("soundUse")]

        public SoundSpecifier SoundUse = new SoundPathSpecifier("/Audio/Nyanotrasen/heartbeat_fast.ogg");

        [DataField("range")]
        public float Range = 5f;

        [DataField("actionWideMetapsionic", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string ActionWideMetapsionic = "ActionWideMetapsionic";

        [DataField("actionWideMetapsionicEntity")]
        public EntityUid? ActionWideMetapsionicEntity;

        [DataField("actionFocusedMetapsionic", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string ActionFocusedMetapsionic = "ActionFocusedMetapsionic";

        [DataField("actionFocusedMetapsionicEntity")]
        public EntityUid? ActionFocusedMetapsionicEntity;

        [DataField("metapsionicFeedback")]
        public string MetapsionicFeedback = "metapsionic-feedback";
    }
}
