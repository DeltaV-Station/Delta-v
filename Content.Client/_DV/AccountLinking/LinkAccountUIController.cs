using Content.Client.Lobby.UI;
using Content.Client.Message;
using Content.Shared._DV.CCVars;
using Content.Shared._DV.AccountLinking;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.LineEdit;
using static Robust.Client.UserInterface.Controls.TabContainer;

namespace Content.Client._DV.AccountLinking;

public sealed class LinkAccountUIController : UIController, IOnSystemChanged<LinkAccountSystem>
{
    [Dependency] private readonly IClipboardManager _clipboard = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly LinkAccountManager _linkAccount = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IUriOpener _uriOpener = default!;

    private LinkAccountWindow? _window;
    private TimeSpan _disableUntil;

    private Guid _code;

    public override void Initialize()
    {
        _linkAccount.CodeReceived += OnCode;
        _linkAccount.Updated += OnUpdated;
    }

    private void OnCode(Guid code)
    {
        _code = code;

        if (_window == null)
            return;

        _window.CopyButton.Disabled = false;
    }

    private void OnUpdated()
    {
        if (UIManager.ActiveScreen is not LobbyGui gui)
            return;

    }

    public void ToggleWindow()
    {
        if (_window == null)
        {
            _window = new LinkAccountWindow();
            _window.OnClose += () => _window = null;
            _window.Label.SetMarkupPermissive($"{Loc.GetString("ui-link-discord-account-text")}");
            if (_linkAccount.Linked)
                _window.Label.SetMarkupPermissive($"{Loc.GetString("ui-link-discord-account-already-linked")}\n\n{Loc.GetString("ui-link-discord-account-text")}");

            _window.CopyButton.OnPressed += _ =>
            {
                _clipboard.SetText(_code.ToString());
                _window.CopyButton.Text = Loc.GetString("ui-link-discord-account-copied");
                _window.CopyButton.Disabled = true;
                _disableUntil = _timing.RealTime.Add(TimeSpan.FromSeconds(3));
            };

            var messageLink = _config.GetCVar(DCCVars.DiscordAccountLinkingMessageLink);
            if (string.IsNullOrEmpty(messageLink))
            {
                _window.LinkButton.Visible = false;
                _window.CopyButton.RemoveStyleClass("OpenRight");
            }
            else
            {
                _window.LinkButton.Visible = true;
                _window.LinkButton.OnPressed += _ => _uriOpener.OpenUri(messageLink);
                _window.CopyButton.AddStyleClass("OpenRight");
            }

            _window.OpenCentered();

            if (_code == default)
                _window.CopyButton.Disabled = true;

            _net.ClientSendMessage(new LinkAccountRequestMsg());
            return;
        }

        _window.Close();
        _window = null;
    }

    public override void FrameUpdate(FrameEventArgs args)
    {
        if (_window == null)
            return;

        var time = _timing.RealTime;
        if (_disableUntil != default && time > _disableUntil)
        {
            _disableUntil = default;
            _window.CopyButton.Text = Loc.GetString("ui-link-discord-account-copy");
            _window.CopyButton.Disabled = false;
        }
    }
}
