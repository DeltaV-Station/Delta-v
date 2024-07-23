using Robust.Shared.Prototypes;

namespace Content.Shared.EinsteinEngines.Actions.Events;

public sealed partial class FabricateActionEvent : InstantActionEvent
{
    [DataField(required: true)]
    public ProtoId<EntityPrototype> Fabrication;
}
