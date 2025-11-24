using Robust.Shared.GameStates;

namespace Content.Shared._DV.Psionics.Components;

/// <summary>
/// Entities with this component are psionically insulated from a source.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PsionicallyInsulatedComponent : Component
{
    /// <summary>
    /// Whether the wearer can still use their own psionics.
    /// </summary>
    /// <example>The mantis' jacket is insulated and allows passthrough, which protects them from psionics while allowing them to use their own.</example>
    [DataField, AutoNetworkedField]
    public bool AllowsPsionicUsage;

    /// <summary>
    /// Whether the wearer is shielded from psionic influences.
    /// </summary>
    /// <example>The security headcage stops the wearer from using their own psionics, but can still be affected by others.</example>
    [DataField, AutoNetworkedField]
    public bool ShieldsFromPsionics = true;
}
