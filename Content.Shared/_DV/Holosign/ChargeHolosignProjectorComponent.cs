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

    /// <summary>
    /// A whitelist component that the projection has to have in order for the projector to pick it back up.
    /// For example, HolosignWetFloor has the component Holosign, and while HoloFan has the Holofan component.
    /// When SignComponentName is Holosign, then it can pick up HolosignWetFloor but not HoloFan.
    /// </summary>
    [DataField]
    public string? SignComponentName = "Holosign";

    public Type SignComponent = default!;

    /// <summary>
    /// If true, the holosign projector can pick up the entity whitelisted in the SignComponentName variable.
    /// </summary>
    [DataField]
    public bool CanPickup = true;
}
