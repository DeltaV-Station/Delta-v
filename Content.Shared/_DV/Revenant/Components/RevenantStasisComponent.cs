using Content.Shared.Revenant.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Revenant.Components;

/// <summary>
/// Used by the RevenantStasis status effect.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class RevenantStasisComponent : Component
{
    [DataField]
    public EntityUid Revenant;
}
