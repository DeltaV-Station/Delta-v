using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

/// <summary>
/// Entities with this component are psionically insulated from a source.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public abstract partial class BasePsionicPower : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    [DataField, AutoNetworkedField]
    public EntProtoId ActionProtoId;
}
