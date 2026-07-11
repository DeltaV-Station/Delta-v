using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CD.Silicons.Borgs;

/// <summary>
/// Component given to borgs that should be able to select subtypes inside of the borg type selection menu.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(true)]
public sealed partial class BorgSwitchableSubtypeComponent : Component
{
    /// <summary>
    /// The <see cref="BorgSubtypeDefinitionComponent"/> of this chassis.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<EntityPrototype>? BorgSubtype;
}
