using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._starcup.Footprints;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FootprintModifierComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<FootprintPrototype>? Footprint;
}
