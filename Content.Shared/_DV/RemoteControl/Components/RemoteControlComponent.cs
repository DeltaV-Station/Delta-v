using Content.Shared._DV.RemoteControl.EntitySystems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.RemoteControl.Components;

/// <summary>
/// Marks an entity as being a remote control for entities, giving them the ability to send orders
/// via pointing.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedRemoteControlSystem))]
public sealed partial class RemoteControlComponent : Component
{
    /// <summary>
    /// The sound to emit when the remote control is used.
    /// </summary>
    [DataField(required: true)]
    public SoundSpecifier UseSound;

    /// <summary>
    /// A string denoting which "Channel" kind this entity can broadcast on.
    /// </summary>
    [DataField(required: true)]
    public string ChannelName;

    /// <summary>
    /// Cooldown time between orders.
    /// </summary>
    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(3);

    /// <summary>
    /// The entities, if any, this remote control is bound to when handling orders.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<EntityUid> BoundEntities = new() { };

    /// <summary>
    /// Whether this remote control can bind to many different receivers.
    /// </summary>
    [DataField]
    public bool AllowMultiple = false;

    /// <summary>
    /// Time it takes to bind an entity to this control;
    /// </summary>
    public TimeSpan BindingTime = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Time it takes to unbind an entity from this control.
    /// </summary>
    public TimeSpan UnbindingTime = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Screeches are shown as a popup when a receiver is on the right channel but
    /// cannot understand the orders.
    /// </summary>
    [DataField]
    public List<string> Screeches = new() { };

    /// <summary>
    /// Dictionary of orders and their respective localised strings to show to the player.
    /// If no order string exists for a given order, a default one will be applied.
    /// </summary>
    [DataField]
    public Dictionary<RemoteControlOrderType, LocId> OrderStrings;

    /// <summary>
    /// Localised string to show when a remote control is toggled without any entities
    /// bound to it.
    /// </summary>
    [DataField]
    public LocId NoBoundEntitiesWarning = "remote-control-no-bound-entities";
}

[Serializable, NetSerializable]
public enum RemoteControlOrderType : byte
{
    EntityPoint, // Pointed at some other entity.
    TilePoint,   // Pointed at a tile on the ground.
    SelfPoint,   // Pointed at themselves.
    FreeUnit,    // This unit has no particular orders.
}
