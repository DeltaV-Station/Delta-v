using Content.Shared.Clothing;
using Content.Shared.Roles;
using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using System;
namespace Content.Shared._DV.Clothing.Components;
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
public sealed partial class JobColorComponent : Component
{
    [DataField]
    [AutoNetworkedField]
    public Dictionary<ProtoId<JobIconPrototype>, Dictionary<string, Color>> JobMap = new();
    [DataField]
    [AutoNetworkedField]
    public ProtoId<JobIconPrototype> CurrentJobIcon = new("JobIconUnknown");
    [DataField]
    public bool ManualChange = true;
}
