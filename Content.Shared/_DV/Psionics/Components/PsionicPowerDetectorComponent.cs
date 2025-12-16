using Robust.Shared.GameStates;

namespace Content.Shared._DV.Psionics.Components;

/// <summary>
/// Entities with this component can detect psionic usage nearby.
/// This is usually paired with the metapsionic pulse power.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PsionicPowerDetectorComponent : Component
{
    /// <summary>
    /// In case the detection is provided by an item, remember the wearer.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Wearer;
}
