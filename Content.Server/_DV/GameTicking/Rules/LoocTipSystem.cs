using Content.Server._DV.Tips;
using Content.Shared._DV.Tips;
using Content.Shared.Chat;
using Robust.Server.Player;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.GameTicking.Rules;

public sealed class LoocTipSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly TipSystem _tips = default!;

    private static readonly ProtoId<TipPrototype> LoocTip = "LoocTip";

    // Hashset of users who already haven seen the hint
    private readonly HashSet<NetUserId> _shownThisSession = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<InGameOocMessageAttemptEvent>(OnInGameLooc);
        _player.PlayerStatusChanged += OnPlayerStatusChanged;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _player.PlayerStatusChanged -= OnPlayerStatusChanged;
    }

    private void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs args)
    {
        if (args.NewStatus == SessionStatus.Disconnected)
            _shownThisSession.Remove(args.Session.UserId);
    }

    private void OnInGameLooc(ref InGameOocMessageAttemptEvent args)
    {
        if (args.Cancelled || args.Type != InGameOOCChatType.Looc)
            return;

        // Do not show if it already was shown
        if (!_shownThisSession.Add(args.Session.UserId))
            return;

        // Force show the tip, even if tips are disabled by the user
        _tips.ShowTip(args.Session, LoocTip, ignoreCvar: true);
    }
}
