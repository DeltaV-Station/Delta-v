using Robust.Shared.Analyzers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Ailments;

/// <summary>
/// A surgery step that attempts to cause the given transition in the given ailment pack
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SurgeryStepAilmentTransitionComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public ProtoId<AilmentPackPrototype> Pack;

    [DataField(required: true), AutoNetworkedField]
    public ProtoId<AilmentTransitionPrototype> Transition;
}
