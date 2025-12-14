using Content.Shared._DV.RemoteControl.Components;
using Content.Shared._DV.RemoteControl.EntitySystems;

namespace Content.Client._DV.RemoteControl.EntitySystems;

/// <summary>
/// Client side handling for remote controls.
/// </summary>
public sealed class RemoteControlSystem : SharedRemoteControlSystem
{
    /// <summary>
    /// Client side handling of setting a unit free.
    /// </summary>
    /// <param name="entity">The entity to set free.</param>
    protected override void SetUnitFree(Entity<RemoteControlReceiverComponent> entity)
    { }
}
