using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Holosign;

/// <summary>
/// A holosign projector that uses <c>LimitedCharges</c> instead of a power cell slot.
/// Currently there is no spawning prediction so signs are spawned once in a container and moved out to allow prediction.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(ChargeHolosignSystem))]
public sealed partial class ChargeHolosignProjectorComponent : Component
{
    /// <summary>
    /// The entity to spawn.
    /// </summary>
    [DataField]
    public EntProtoId SignProto = "HolosignWetFloor";

    [DataField]
    public string? SignComponentName = "Holosign";

    public Type SignComponent = default!;

    /// <summary>
    /// If true, the holosign projector can pick up the entity whitelisted in the SignComponentName variable.
    /// </summary>
    [DataField]
    public bool CanPickup = true;
}
