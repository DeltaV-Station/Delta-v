using Robust.Shared.Prototypes;

namespace Content.Shared.Actions.Events;

public sealed partial class FabricateActionEvent : InstantActionEvent
{
    [DataField(required: true)]
    public ProtoId<EntityPrototype> Fabrication;
}
