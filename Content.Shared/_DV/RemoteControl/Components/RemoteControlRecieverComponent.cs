using Content.Shared._DV.RemoteControl.EntitySystems;

namespace Content.Shared._DV.RemoteControl.Components;

/// <summary>
/// Marks that this entity can recieve, and optionally understand, remote control orders.
/// </summary>
[RegisterComponent]
[Access(typeof(SharedRemoteControlSystem))]
public sealed partial class RemoteControlReceiverComponent : Component
{
    /// <summary>
    /// Whether this entity can understand the commands given by a remote control.
    /// </summary>
    [DataField]
    public bool CanUnderstand = false;

    /// <summary>
    /// A string denoting which "Channel" kind this entity can receive on.
    /// </summary>
    [DataField(required: true)]
    public string ChannelName;

    /// <summary>
    /// Dictionary of orders and their respective localised strings to show to the player.
    /// If no order string exists for a given order, a default one will be applied.
    /// </summary>
    [DataField]
    public Dictionary<RemoteControlOrderType, LocId> OrderStrings;
}
