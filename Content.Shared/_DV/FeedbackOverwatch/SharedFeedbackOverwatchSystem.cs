using Content.Shared.Mind;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.FeedbackOverwatch;

public sealed partial class SharedFeedbackOverwatchSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public List<string> FeedbackPopupProtoIds { get; } = new();

    public override void Initialize()
    {
        InitializeEvents();
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);

        LoadPrototypes();
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (!args.WasModified<FeedbackPopupPrototype>())
            return;

        LoadPrototypes();
    }

    /// <summary>
    ///     Load all the prototype IDs into FeedbackPopupProtoIds.
    /// </summary>
    private void LoadPrototypes()
    {
        FeedbackPopupProtoIds.Clear();
        var protos = _proto.EnumeratePrototypes<FeedbackPopupPrototype>();
        foreach (var proto in protos)
            FeedbackPopupProtoIds.Add(proto.ID);
        FeedbackPopupProtoIds.Sort();
    }

    /// <summary>
    ///     Send a popup to the given client controlling the given UID.
    /// </summary>
    /// <param name="uid">UID of the entity the player is controlling.</param>
    /// <param name="popupPrototype">Popup to send them.</param>
    /// <param name="sendOnlyOnce">If true, if the popup is sent to the same mind again, it will not be displayed.</param>
    /// <returns>Returns true if the popup message was sent to the client successfully.</returns>
    public bool SendPopup(EntityUid? uid, ProtoId<FeedbackPopupPrototype> popupPrototype, bool sendOnlyOnce = true)
    {
        if (uid == null)
            return false;

        if (!_mind.TryGetMind(uid.Value, out var mindUid, out _))
            return false;

        return SendPopupMind(mindUid, popupPrototype, sendOnlyOnce);
    }

    /// <summary>
    ///     Send a popup to the given client controlling the given mind.
    /// </summary>
    /// <param name="uid">UID of the players mind.</param>
    /// <param name="popupPrototype">Popup to send them.</param>
    /// <param name="sendOnlyOnce">If true, if the popup is sent to the same mind again, it will not be displayed.</param>
    /// <returns>Returns true if the popup message was sent to the client successfully.</returns>
    public bool SendPopupMind(EntityUid? uid, ProtoId<FeedbackPopupPrototype> popupPrototype, bool sendOnlyOnce = true)
    {
        if (uid == null)
            return false;

        if (sendOnlyOnce)
        {
            EnsureComp<FeedbackPopupInformationComponent>(uid.Value, out var feedbackInfoComp);

            // If it's already been seen, don't resend it.
            if (!feedbackInfoComp.SeenPopups.Add(popupPrototype))
                return false;

            Dirty(uid.Value, feedbackInfoComp);
        }

        if (!_mind.TryGetSession(uid, out var session))
            return false;

        var msg = new FeedbackPopupMessage(popupPrototype);
        RaiseNetworkEvent(msg, session);
        return true;
    }
}
