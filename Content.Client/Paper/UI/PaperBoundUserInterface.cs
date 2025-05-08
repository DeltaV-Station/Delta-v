using JetBrains.Annotations;
using Content.Client.Chat.TypingIndicator;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Content.Shared.Chat.TypingIndicator;
using Robust.Shared.Utility;
using Content.Shared.Paper;
using static Content.Shared.Paper.PaperComponent;
using Robust.Shared.Prototypes;

namespace Content.Client.Paper.UI;

[UsedImplicitly]
public sealed class PaperBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private PaperWindow? _window;

    private static readonly ProtoId<TypingIndicatorPrototype> TypingIndicator = "paper";

    private TypingIndicatorSystem? _typing;

    public PaperBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<PaperWindow>();
        _window.OnSaved += InputOnTextEntered;
        _window.Typing += OnTyping;
        _window.SubmitPressed += OnSubmit;

        if (EntMan.TryGetComponent<PaperComponent>(Owner, out var paper))
        {
            _window.MaxInputLength = paper.ContentSize;
        }
        if (EntMan.TryGetComponent<PaperVisualsComponent>(Owner, out var visuals))
        {
            _window.InitVisuals(Owner, visuals);
        }
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        _window?.Populate((PaperBoundUserInterfaceState) state);
    }

    private void InputOnTextEntered(string text)
    {
        SendMessage(new PaperInputTextMessage(text));

        if (_window != null)
        {
            _window.Input.TextRope = Rope.Leaf.Empty;
            _window.Input.CursorPosition = new TextEdit.CursorPos(0, TextEdit.LineBreakBias.Top);
        }
    }

    private void OnTyping()
    {
        _typing ??= EntMan.System<TypingIndicatorSystem>();
        _typing?.ClientAlternateTyping(TypingIndicator);
    }

    private void OnSubmit()
    {
        _typing ??= EntMan.System<TypingIndicatorSystem>();
        _typing?.ClientSubmittedChatText();
    }
}
