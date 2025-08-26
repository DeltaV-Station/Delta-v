using Content.Shared.DeviceLinking;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Content.Shared._DV.DeviceLinking.Systems;

namespace Content.Shared._DV.DeviceLinking.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(PowerTogglePortSystem))]
public sealed partial class PowerTogglePortComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<SinkPortPrototype> PowerTogglePort = "PowerToggle";

};
