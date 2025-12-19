using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Abilities.Psionics
{
    [RegisterComponent]
    public sealed partial class MassSleepPowerComponent : Component
    {
        public float Radius = 1.25f;
        [DataField("massSleepActionId",
        customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? MassSleepActionId = "ActionMassSleep";

        [DataField("massSleepActionEntity")]
        public EntityUid? MassSleepActionEntity;

        [DataField]
        public DoAfterId? DoAfter;

        [DataField]
        public TimeSpan UseDelay = TimeSpan.FromSeconds(4);

        [DataField]
        public float Duration = 5f;

        [DataField]
        public float WarningRadius = 6f;
    }
}
