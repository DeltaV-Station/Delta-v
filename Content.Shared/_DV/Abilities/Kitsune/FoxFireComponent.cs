using Robust.Shared.GameStates;

namespace Content.Shared._DV.Abilities.Kitsune;

/// <summary>
/// This component is needed on fox fires so that the owner can properly update upon its destruction.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class FoxfireComponent : Component
{
    /// <summary>
    /// The kitsune that created this fox fire.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Kitsune;
}
