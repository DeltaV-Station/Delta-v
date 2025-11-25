using Robust.Shared.GameStates;

namespace Content.Shared._DV.Psionics.Components;

/// <summary>
/// Entities with this component are psionics and can use powers.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PsionicComponent : Component
{
    /// <summary>
    /// The list of the action buttons for every power.
    /// </summary>
    [DataField]
    public HashSet<EntityUid?> PsionicPowersActionEntities = [];

    /// <summary>
    /// Whether the psionic power can be removed from them.
    /// </summary>
    /// <example>Revenants shouldn't be able to lose their powers.</example>
    [DataField, AutoNetworkedField]
    public bool Removable = true;
}
