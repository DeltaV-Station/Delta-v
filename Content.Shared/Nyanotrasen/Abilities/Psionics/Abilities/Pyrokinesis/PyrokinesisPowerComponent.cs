using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Abilities.Psionics
{
    [RegisterComponent]
    public sealed partial class PyrokinesisPowerComponent : Component
    {
        public EntityTargetActionComponent? PyrokinesisPowerAction = null;
        [DataField("pyrokinesisActionId",
        customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? PyrokinesisActionId = "ActionPyrokinesis";

        [DataField("pyrokinesisActionEntity")]
        public EntityUid? PyrokinesisActionEntity;
    }
}
