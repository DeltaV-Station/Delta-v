using Robust.Shared.GameStates;

namespace Content.Shared._DV.Psionics.Components;

/// <summary>
/// Entities with this component are psionics and can use powers.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PsionicComponent : Component
{
    /// <summary>
    /// The list of the action buttons for every power.
    /// </summary>
    [DataField]
    public HashSet<EntityUid?> PsionicPowersActionEntities = [];

    /// <summary>
    /// Whether the psionic gets stunned when a psionic power gets removed. This doesn't mean they lost all psionic powers.
    /// Psionic powers themselves regulate if they can be removed.
    /// </summary>
    [DataField]
    public bool StunOnRemoval = true;
}
