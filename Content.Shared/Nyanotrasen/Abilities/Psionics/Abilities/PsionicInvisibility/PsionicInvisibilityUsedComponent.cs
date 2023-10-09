using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
namespace Content.Shared.Abilities.Psionics
{
    [RegisterComponent]
    public sealed partial class PsionicInvisibilityUsedComponent : Component
    {
        [ValidatePrototypeId<EntityPrototype>]
        public const string PsionicInvisibilityUsedActionPrototype = "ActionPsionicInvisibilityUsed";
        [DataField("psionicInvisibilityUsedActionId",
        customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? PsionicInvisibilityUsedActionId = "ActionPsionicInvisibilityUsed";

        [DataField("psionicInvisibilityUsedActionEntity")]
        public EntityUid? PsionicInvisibilityUsedActionEntity;
    }
}
