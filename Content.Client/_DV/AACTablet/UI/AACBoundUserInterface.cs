using Content.Client.Chat.TypingIndicator;
using Content.Shared._DV.AACTablet;
using Content.Shared._DV.QuickPhrase;
using Content.Shared.Chat.TypingIndicator;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._DV.AACTablet.UI;

public sealed class AACBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private AACWindow? _window;

    private static readonly ProtoId<TypingIndicatorPrototype> AACTypingIndicator = "aac";

    public AACBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _window?.Close();
        _window = this.CreateWindow<AACWindow>();
        _window.PhraseButtonPressed += OnPhraseButtonPressed;
        _window.Typing += OnTyping;
        _window.SubmitPressed += OnSubmit;
    }

    private void OnPhraseButtonPressed(List<ProtoId<QuickPhrasePrototype>> phraseId)
    {
        SendMessage(new AACTabletSendPhraseMessage(phraseId));
    }

    private void OnTyping()
    {
        if (!EntMan.TrySystem(out TypingIndicatorSystem? typingIndicatorSystem))
            return;

        typingIndicatorSystem.ClientAlternateTyping(AACTypingIndicator);
    }

    private void OnSubmit()
    {
        if (!EntMan.TrySystem(out TypingIndicatorSystem? typingIndicatorSystem))
            return;

        typingIndicatorSystem.ClientSubmittedChatText();
    }
}
