using Content.Server.Chat.Systems;
using Content.Shared.DeltaV.AACTablet;
using Content.Shared.DeltaV.QuickPhrase;
using Robust.Shared.Prototypes;

namespace Content.Server.DeltaV.AACTablet;

public sealed class AACTabletSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly ILocalizationManager _loc = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AACTabletComponent, AACTabletSendPhraseMessage>(OnSendPhrase);
    }

    private void OnSendPhrase(EntityUid uid, AACTabletComponent component, AACTabletSendPhraseMessage message)
    {
        var phrase = _prototypeManager.Index<QuickPhrasePrototype>(message.PhraseID);

        _chat.TrySendInGameICMessage(uid, _loc.GetString(phrase.Text), InGameICChatType.Speak, false);
    }
}