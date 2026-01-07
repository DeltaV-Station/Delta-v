using System.Threading;
using Content.Shared.Storage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Mail;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedMailSystem))]
public sealed partial class MailComponent : Component
{
    /// <summary>
    /// The full name of the person who should receive this mail.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string Recipient = "Unknown";

    /// <summary>
    /// The job title of the person who should receive this mail.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string RecipientJob = "Unknown";

    /// <summary>
    /// Whether this mail is currently locked.
    /// </summary>
    /// <remarks>
    /// Why do we not use LockComponent?
    /// Because this can't be locked again,
    /// and we have special conditions for unlocking,
    /// and we don't want to add a verb.
    /// </remarks>
    [DataField, AutoNetworkedField]
    public bool IsLocked = true;

    /// <summary>
    /// Is this parcel profitable to deliver for the station?
    /// </summary>
    /// <remarks>
    /// The station won't receive any award on delivery if this is false.
    /// This is useful for broken fragile packages and packages that were
    /// not delivered in time.
    /// </remarks>
    [DataField, AutoNetworkedField]
    public bool IsProfitable = true;

    /// <summary>
    /// Is this package considered fragile?
    /// </summary>
    /// <remarks>
    /// This can be set to true in the YAML files for a mail delivery to
    /// always be Fragile, despite its contents.
    /// </remarks>
    [DataField, AutoNetworkedField]
    public bool IsFragile;

    /// <summary>
    /// Is this package considered priority mail?
    /// </summary>
    /// <remarks>
    /// There will be a timer set for its successful delivery. The
    /// station's bank account will be penalized if it is not delivered on
    /// time.
    ///
    /// This is set to false on successful delivery.
    ///
    /// This can be set to true in the YAML files for a mail delivery to
    /// always be Priority.
    /// </remarks>
    [DataField, AutoNetworkedField]
    public bool IsPriority;

    /// <summary>
    /// The time when this priority mail expires if Priority.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan? ExpiryTime;

    // Frontier Mail Port: large mail
    /// <summary>
    /// Whether this parcel is large.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsLarge;
    // End Frontier: large mail

    /// <summary>
    /// What will be packaged when the mail is spawned.
    /// </summary>
    [DataField]
    public List<EntitySpawnEntry> Contents = new();

    /// <summary>
    /// The amount that cargo will be awarded for delivering this mail.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Bounty = 750;

    /// <summary>
    /// Penalty if the mail is destroyed.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Penalty = -250;

    /// <summary>
    /// The sound that's played when the mail's lock is broken.
    /// </summary>
    [DataField]
    public SoundSpecifier PenaltySound = new SoundPathSpecifier("/Audio/Machines/Nuke/angry_beep.ogg");

    /// <summary>
    /// The sound that's played when the mail's opened.
    /// </summary>
    [DataField]
    public SoundSpecifier OpenSound = new SoundPathSpecifier("/Audio/Effects/packetrip.ogg");

    /// <summary>
    /// The sound that's played when the mail's lock has been emagged.
    /// </summary>
    [DataField]
    public SoundSpecifier EmagSound = new SoundCollectionSpecifier("sparks");

    /// <summary>
    /// Cancellation token for the priority mail timer.
    /// Used to cancel the penalty timer when priority mail is successfully delivered.
    /// </summary>
    public CancellationTokenSource? PriorityCancelToken;
}
