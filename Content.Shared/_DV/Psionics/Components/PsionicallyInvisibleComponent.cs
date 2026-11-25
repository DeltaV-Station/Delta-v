using Robust.Shared.GameStates;

namespace Content.Shared._DV.Psionics.Components;

/// <summary>
/// This is not the power, this makes you just invisible to anyone potentially psionic.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PsionicallyInvisibleComponent : Component
{
    /// <summary>
    /// Whether the invisibility is currently active.
    /// This can be toggled via psionically surpressing gear.
    /// </summary>
    [DataField]
    public bool Active = true;
}
