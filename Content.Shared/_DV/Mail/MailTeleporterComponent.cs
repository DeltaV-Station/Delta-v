using Content.Shared.Radio;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.Mail;

/// <summary>
/// This is for the mail teleporter.
/// Random mail will be teleported to this every few minutes.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause, Access(typeof(SharedMailSystem))]
public sealed partial class MailTeleporterComponent : Component
{
    /// <summary>
    /// The TimeSpan of next Delivery
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextDelivery = TimeSpan.Zero;

    /// <summary>
    /// The time between new deliveries.
    /// </summary>
    [DataField]
    public TimeSpan TeleportInterval = TimeSpan.FromMinutes(5);

    /// <summary>
    /// The sound that's played when new mail arrives.
    /// </summary>
    [DataField]
    public SoundSpecifier TeleportSound = new SoundPathSpecifier("/Audio/Effects/teleport_arrival.ogg");

    /// <summary>
    /// The MailDeliveryPoolPrototype that's used to select what mail this
    /// teleporter can deliver.
    /// </summary>
    [DataField]
    public ProtoId<MailDeliveryPoolPrototype> MailPool = "RandomDeltaVMailDeliveryPool";

    /// <summary>
    /// Whether the telepad should output a message upon spawning mail.
    /// </summary>
    [DataField]
    public bool RadioNotification;

    /// <summary>
    /// <see cref="LocId"/> to send when spawning new mail.
    /// </summary>
    [DataField]
    public LocId ShipmentReceivedMessage = "mail-received-message";

    /// <summary>
    /// <see cref="RadioChannelPrototype"/> to notify when spawning new mail.
    /// </summary>
    [DataField]
    public ProtoId<RadioChannelPrototype> RadioChannel = "Supply";

    /// <summary>
    /// How many mail candidates do we need per actual delivery sent when
    /// the mail goes out? The number of candidates is divided by this number
    /// to determine how many deliveries will be teleported in.
    /// It does not determine unique recipients. That is random.
    /// </summary>
    [DataField]
    public int CandidatesPerDelivery = 8;

    [DataField]
    public int MinimumDeliveriesPerTeleport = 1;

    /// <summary>
    /// Do not teleport any more mail in, if there are at least this many
    /// undelivered parcels.
    /// </summary>
    /// <remarks>
    /// Currently this works by checking how many MailComponent entities
    /// are sitting on the teleporter's tile.
    ///
    /// It should be noted that if the number of actual deliveries to be
    /// made based on the number of candidates divided by candidates per
    /// delivery exceeds this number, the teleporter will spawn more mail
    /// than this number.
    ///
    /// This is just a simple check to see if anyone's been picking up the
    /// mail lately to prevent entity bloat for the sake of performance.
    /// </remarks>
    [DataField]
    public int MaximumUndeliveredParcels = 5;

    /// <summary>
    /// Any item that breaks or is destroyed in less than this amount of
    /// damage is one of the types of items considered fragile.
    /// </summary>
    [DataField]
    public int FragileDamageThreshold = 10;

    /// <summary>
    /// What's the bonus for delivering a fragile package intact?
    /// </summary>
    [DataField]
    public int FragileBonus = 100;

    /// <summary>
    /// What's the malus for failing to deliver a fragile package?
    /// </summary>
    [DataField]
    public int FragileMalus = -100;

    /// <summary>
    /// What's the chance for any one delivery to be marked as priority mail?
    /// </summary>
    [DataField]
    public float PriorityChance = 0.1f;

    /// <summary>
    /// How long until a priority delivery is considered as having failed
    /// if not delivered?
    /// </summary>
    [DataField]
    public TimeSpan PriorityDuration = TimeSpan.FromMinutes(5);

    /// <summary>
    /// What's the bonus for delivering a priority package on time?
    /// </summary>
    [DataField]
    public int PriorityBonus = 250;

    /// <summary>
    /// What's the malus for failing to deliver a priority package?
    /// </summary>
    [DataField]
    public int PriorityMalus = -250;

    /// <summary>
    /// What's the bonus for delivering a large package intact?
    /// </summary>
    [DataField]
    public int LargeBonus = 1500;

    /// <summary>
    /// What's the malus for failing to deliver a large package?
    /// </summary>
    [DataField]
    public int LargeMalus = -500;
}
