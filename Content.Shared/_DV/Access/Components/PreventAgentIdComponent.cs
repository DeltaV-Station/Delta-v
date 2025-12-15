using Robust.Shared.GameStates;

namespace Content.Shared._DV.Access.Components;

/// <summary>
/// Component to denote the ID card as one specific to a borg.
/// Used to prevent agent IDs from copying access from borg IDs chips.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PreventAgentIdComponent : Component
{
    [DataField]
    public LocId? PopupText = "agent-id-card-cannot-copy";
}
