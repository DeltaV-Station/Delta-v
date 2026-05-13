using Content.Server.Administration.Logs;
using Content.Server.Database;
using Content.Server.GameTicking;
using Content.Shared.Database;
using Content.Shared.RoundEnd;
using Robust.Server.Player;
using Robust.Shared.Player;
using Content.Server.Chat.Managers;

namespace Content.Server.RoundEnd;

/// <summary>
/// Server-side system that handles RP commendation votes from clients.
/// Listens for CommendPlayerMessage from clients and records them in the database.
/// </summary>
public sealed class CommendationSystem : EntitySystem
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<CommendPlayerMessage>(OnCommendPlayer);
    }

    private async void OnCommendPlayer(CommendPlayerMessage message, EntitySessionEventArgs args)
    {
        var senderSession = args.SenderSession;
        var senderUserId = senderSession.UserId;

        // Resolve receiver by OOC name
        if (!_playerManager.TryGetSessionByUsername(message.ReceiverOOCName, out var receiverSession))
        {
            RaiseNetworkEvent(new CommendationSentMessage(false, "Player not found."), senderSession);
            return;
        }

        var receiverUserId = receiverSession.UserId;

        // Anti-abuse: cannot commend yourself
        if (senderUserId == receiverUserId)
        {
            RaiseNetworkEvent(new CommendationSentMessage(false, "You cannot commend yourself."), senderSession);
            return;
        }

        var roundId = _gameTicker.RoundId;

        // Try to insert into database (returns false if already voted this round)
        var success = await _db.AddCommendation(roundId, senderUserId.UserId, receiverUserId.UserId);

        if (!success)
        {
            RaiseNetworkEvent(new CommendationSentMessage(false, "You have already commended someone this round."), senderSession);
            return;
        }

        // Log for admins
        _adminLog.Add(LogType.Action, LogImpact.Low,
            $"RP Commendation: {senderSession.Name} ({senderUserId}) commended {receiverSession.Name} ({receiverUserId}) in round {roundId}");

        // Notify the receiver
        _chatManager.DispatchServerMessage(receiverSession, $"Вас щойно похвалили за гарний відіграш у раунді #{roundId}! Дякуємо за вашу гру!");

        RaiseNetworkEvent(new CommendationSentMessage(true), senderSession);
    }
}
