using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Abilities.Psionics
{
    [RegisterComponent]
    public sealed partial class MetapsionicPowerComponent : Component
    {
        [DataField("range")]
        public float Range = 5f;

        public InstantActionComponent? MetapsionicPowerAction = null;
        [DataField("metapsionicActionId",
        customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? MetapsionicActionId = "ActionMetapsionic";

        [DataField("metapsionicActionEntity")]
        public EntityUid? MetapsionicActionEntity;
    }
}
