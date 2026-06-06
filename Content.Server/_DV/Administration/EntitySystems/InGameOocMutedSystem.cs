using Content.Server._DV.Administration.Components;
using Content.Server.GameTicking;
using Content.Shared.Chat;
using Content.Shared.Players;

namespace Content.Server._DV.Administration.EntitySystems;

public sealed class InGameOocMutedSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InGameOocMessageAttemptEvent>(OnIngameOoc);
    }

    private void OnIngameOoc(ref InGameOocMessageAttemptEvent args)
    {
        if (args.Session.GetMind() is not { } mind)
            return;

        if (!TryComp<InGameOocMutedComponent>(mind, out var oocMuted))
            return;

        if (oocMuted.MuteDeadchat && args.Type == InGameOOCChatType.Dead
            || oocMuted.MuteLooc && args.Type == InGameOOCChatType.Looc)
        {
            args.Cancelled = true;
        }
    }
}
