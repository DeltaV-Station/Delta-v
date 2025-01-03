using Content.Shared.Mind;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.FeedbackOverwatch;

public sealed partial class SharedFeedbackOverwatchSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    public override void Initialize()
    {
        InitializeEvents();
    }

    /// <summary>
    ///     Send a popup to the given client controlling the given UID.
    /// </summary>
    /// <param name="uid">UID of the entity the player is controlling.</param>
    /// <param name="popupPrototype">Popup to send them.</param>
    public void SendPopup(EntityUid? uid, ProtoId<FeedbackPopupPrototype> popupPrototype)
    {
        if (uid == null)
            return;

        if (!_mind.TryGetMind(uid.Value, out var mindUid, out _))
            return;

        SendPopupMind(mindUid, popupPrototype);
    }

    /// <summary>
    ///     Send a popup to the given client controlling the given mind.
    /// </summary>
    /// <param name="uid">UID of the players mind.</param>
    /// <param name="popupPrototype">Popup to send them.</param>
    public void SendPopupMind(EntityUid? uid, ProtoId<FeedbackPopupPrototype> popupPrototype)
    {
        if (uid == null)
            return;

        if (!_mind.TryGetSession(uid, out var session))
            return;

        var msg = new FeedbackPopupMessage(popupPrototype);
        RaiseNetworkEvent(msg, session);
    }
}
