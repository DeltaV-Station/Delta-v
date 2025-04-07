using Robust.Shared.GameStates;

namespace Content.Shared._DV.Abilities.Kitsune;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedKitsuneSystem))]
[AutoGenerateComponentState]
public sealed partial class FoxfireComponent : Component
{
    /// <summary>
    /// The kitsune that created this fox fire.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Kitsune;
}
