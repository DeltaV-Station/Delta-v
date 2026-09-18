using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Harmony.Conspirators.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ConspiratorLeaderComponent : Component
{
    [DataField]
    public ProtoId<FactionIconPrototype> ConspiratorIcon = "ConspiratorLeaderFaction";

    public override bool SessionSpecific => true;
}
