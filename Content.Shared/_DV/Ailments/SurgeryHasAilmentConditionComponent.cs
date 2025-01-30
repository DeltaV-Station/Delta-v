using Robust.Shared.Analyzers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Ailments;

/// <summary>
/// Requires that this part has an ailment for the surgery to be done
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SurgeryHasAilmentConditionComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public ProtoId<AilmentPackPrototype> Pack;

    [DataField(required: true), AutoNetworkedField]
    public ProtoId<AilmentPrototype> Ailment;
}
