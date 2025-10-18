using Robust.Shared.Audio;
using Robust.Shared.Map;

namespace Content.Shared._DV.RemoteControl.Events;

/// <summary>
/// Base event for all orders available to remote controls
/// </summary>
/// <param name="user">The user who activated the remote control.</param>
/// <param name="control">The remote control the activation came from.</param>
/// <param name="boundNPC">Which NPC entities, if any, this control is bound to.</param>
public abstract class BaseRemoteControlOrderEvent(EntityUid user, EntityUid control, List<EntityUid> boundNPCs)
{
    /// <summary>
    /// The user who activated the remote control.
    /// </summary>
    public EntityUid User = user;

    /// <summary>
    /// The remote control that was activated.
    /// </summary>
    public EntityUid Control = control;

    /// <summary>
    /// Which NPCs are bound to this remote, if any.
    /// </summary>
    public List<EntityUid> BoundNPCs = boundNPCs;
};

[ByRefEvent]
public sealed class RemoteControlEntityPointOrderEvent(
    EntityUid user,
    EntityUid control,
    List<EntityUid> boundNPCs,
    EntityUid target)
    : BaseRemoteControlOrderEvent(user, control, boundNPCs)
{
    /// <summary>
    /// Entity the order is associated with.
    /// </summary>
    public EntityUid Target = target;
}

[ByRefEvent]
public sealed class RemoteControlTilePointOrderEvent(
    EntityUid user,
    EntityUid control,
    List<EntityUid> boundNPCs,
    MapCoordinates location)
    : BaseRemoteControlOrderEvent(user, control, boundNPCs)
{
    /// <summary>
    /// Map coordinates the order points to.
    /// </summary>
    public MapCoordinates Location = location;
}

[ByRefEvent]
public sealed class RemoteControlSelfPointOrderEvent(EntityUid user, EntityUid control, List<EntityUid> boundNPCs)
    : BaseRemoteControlOrderEvent(user, control, boundNPCs);

[ByRefEvent]
public sealed class RemoteControlFreeUnitOrderEvent(EntityUid user, EntityUid control, List<EntityUid> boundNPCs)
    : BaseRemoteControlOrderEvent(user, control, boundNPCs);
