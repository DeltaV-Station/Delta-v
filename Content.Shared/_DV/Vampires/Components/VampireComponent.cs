using Content.Shared._DV.Vampires.EntitySystems;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Vampires.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedVampireSystem))]
public sealed partial class VampireComponent : Component
{
    /// <summary>
    /// The timestamp at which this vampire last drained a victim's blood.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public TimeSpan LastDrainedTime;

    /// <summary>
    /// How long blood should still visible on the vampire after draining blood.
    /// </summary>
    [DataField]
    public TimeSpan DrainVisibleDuration = TimeSpan.FromSeconds(10);
}
