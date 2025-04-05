using Robust.Shared.GameStates;

namespace Content.Shared._DV.Forensics.Components;

/// <summary>
/// Applies an identifier to weapons and the bullets/projectiles fired from them.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RiflingForensicsComponent : Component
{
    /// <summary>
    /// Unique identifier generated at random, much like DNA and fingerprints.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? Identifier = null;
}
