using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Content.Client._DV.Curation.Systems;
using Content.Client._DV.Curation.UI.Cwoink;
using Content.Client.Administration.Managers;
using Content.Client.Gameplay;
using Content.Client.Lobby;
using Content.Client.Stylesheets;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.MenuBar.Widgets;
using Content.Shared._DV.CCVars;
using Content.Shared._DV.Curation;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Content.Shared.Input;
using JetBrains.Annotations;
using Robust.Client.Audio;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Audio;
using Robust.Shared.Configuration;
using Robust.Shared.Input.Binding;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Client._DV.UserInterfaces.Systems.Cwoink;

[UsedImplicitly]
public sealed class CHelpUIController : UIController, IOnSystemChanged<CwoinkSystem>, IOnStateChanged<GameplayState>, IOnStateChanged<LobbyState>
{
    [Dependency] private readonly IClientAdminManager _adminManager = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
    [UISystemDependency] private readonly AudioSystem _audio = default!;

    private CwoinkSystem? _cwoinkSystem;
    private MenuButton? GameCHelpButton => UIManager.GetActiveUIWidgetOrNull<GameTopMenuBar>()?.CHelpButton;
    public ICHelpUIHandler? UIHelper;
    private bool _discordRelayActive;
    private bool _hasUnreadCHelp;
    private bool _cwoinkSoundEnabled;
    private SoundPathSpecifier? _cHelpSound;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<CwoinkDiscordRelayUpdated>(DiscordRelayUpdated);
        SubscribeNetworkEvent<CwoinkPlayerTypingUpdated>(PeopleTypingUpdated);

        _adminManager.AdminStatusUpdated += OnAdminStatusUpdated;
        _config.OnValueChanged(DCCVars.CHelpSound, v => _cHelpSound = v, true);
        _config.OnValueChanged(CCVars.BwoinkSoundEnabled, v => _cwoinkSoundEnabled = v, true);
    }

    public void UnloadButton()
    {
        if (GameCHelpButton != null)
            GameCHelpButton.OnPressed -= CHelpButtonPressed;
    }

    public void LoadButton()
    {
        if (GameCHelpButton != null)
            GameCHelpButton.OnPressed += CHelpButtonPressed;
    }

    private void OnAdminStatusUpdated()
    {
        if (UIHelper is not { IsOpen: true })
            return;
        EnsureUIHelper();
    }

    private void CHelpButtonPressed(BaseButton.ButtonEventArgs obj)
    {
        EnsureUIHelper();
        UIHelper!.ToggleWindow();
    }

    public void OnSystemLoaded(CwoinkSystem system)
    {
        _cwoinkSystem = system;
        _cwoinkSystem.OnCwoinkTextMessageReceived += ReceivedCwoink;

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.OpenCHelp,
                InputCmdHandler.FromDelegate(_ => ToggleWindow()))
            .Register<CHelpUIController>();
    }

    public void OnSystemUnloaded(CwoinkSystem system)
    {
        CommandBinds.Unregister<CHelpUIController>();

        DebugTools.Assert(_cwoinkSystem != null);
        _cwoinkSystem!.OnCwoinkTextMessageReceived -= ReceivedCwoink;
        _cwoinkSystem = null;
    }

    private void SetCHelpPressed(bool pressed)
    {
        if (GameCHelpButton != null)
        {
            GameCHelpButton.Pressed = pressed;
        }

        UIManager.ClickSound();
        UnreadCHelpRead();
    }

    private void ReceivedCwoink(object? sender, CwoinkTextMessage message)
    {
        Logger.GetSawmill("c.s.go.es.cwoink").Info($"@{message.UserId}: {message.Text}");
        var localPlayer = _playerManager.LocalSession;
        if (localPlayer == null)
        {
            return;
        }
        if (message.PlaySound && localPlayer.UserId != message.TrueSender)
        {
            if (_cHelpSound != null && (_cwoinkSoundEnabled || !_adminManager.IsActive()))
                _audio.PlayGlobal(_cHelpSound, Filter.Local(), false);
            _clyde.RequestWindowAttention();
        }

        EnsureUIHelper();

        if (!UIHelper!.IsOpen)
        {
            UnreadCHelpReceived();
        }

        UIHelper!.Receive(message);
    }

    private void DiscordRelayUpdated(CwoinkDiscordRelayUpdated args, EntitySessionEventArgs session)
    {
        _discordRelayActive = args.DiscordRelayEnabled;
        UIHelper?.DiscordRelayChanged(_discordRelayActive);
    }

    private void PeopleTypingUpdated(CwoinkPlayerTypingUpdated args, EntitySessionEventArgs session)
    {
        UIHelper?.PeopleTypingUpdated(args);
    }

    public void EnsureUIHelper()
    {
        var isCurator = _adminManager.HasFlag(AdminFlags.CuratorHelp);

        if (UIHelper != null && UIHelper.IsCurator == isCurator)
            return;

        UIHelper?.Dispose();
        var ownerUserId = _playerManager.LocalUser!.Value;
        UIHelper = isCurator ? new CuratorCHelpUIHandler(ownerUserId) : new UserCHelpUIHandler(ownerUserId);
        UIHelper.DiscordRelayChanged(_discordRelayActive);

        UIHelper.SendMessageAction = (userId, textMessage, playSound, adminOnly) => _cwoinkSystem?.Send(userId, textMessage, playSound, adminOnly);
        UIHelper.InputTextChanged += (channel, text) => _cwoinkSystem?.SendInputTextUpdated(channel, text.Length > 0);
        UIHelper.OnClose += () => SetCHelpPressed(false);
        UIHelper.OnOpen += () => SetCHelpPressed(true);
        SetCHelpPressed(UIHelper.IsOpen);
    }

    public void Open()
    {
        var localUser = _playerManager.LocalUser;
        if (localUser == null)
            return;
        EnsureUIHelper();
        if (UIHelper!.IsOpen)
            return;
        UIHelper!.Open(localUser.Value, _discordRelayActive);
    }

    public void Open(NetUserId userId)
    {
        EnsureUIHelper();
        if (!UIHelper!.IsCurator)
            return;
        UIHelper?.Open(userId, _discordRelayActive);
    }

    public void ToggleWindow()
    {
        EnsureUIHelper();
        UIHelper?.ToggleWindow();
    }

    public void PopOut()
    {
        EnsureUIHelper();
        if (UIHelper is not CuratorCHelpUIHandler helper)
            return;

        if (helper.Window == null || helper.Control == null)
        {
            return;
        }

        helper.Control.Orphan();
        helper.Window.Dispose();
        helper.Window = null;
        helper.EverOpened = false;

        var monitor = _clyde.EnumerateMonitors().First();

        helper.ClydeWindow = _clyde.CreateWindow(new WindowCreateParameters
        {
            Maximized = false,
            Title = "Curator Help",
            Monitor = monitor,
            Width = 900,
            Height = 500
        });

        helper.ClydeWindow.RequestClosed += helper.OnRequestClosed;
        helper.ClydeWindow.DisposeOnClose = true;

        helper.WindowRoot = _uiManager.CreateWindowRoot(helper.ClydeWindow);
        helper.WindowRoot.AddChild(helper.Control);

        helper.Control.PopOut.Disabled = true;
        helper.Control.PopOut.Visible = false;
    }

    private void UnreadCHelpReceived()
    {
        GameCHelpButton?.StyleClasses.Add(StyleClass.Negative);
        _hasUnreadCHelp = true;
    }

    private void UnreadCHelpRead()
    {
        GameCHelpButton?.StyleClasses.Remove(StyleClass.Negative);
        _hasUnreadCHelp = false;
    }

    public void OnStateEntered(GameplayState state)
    {
        if (GameCHelpButton != null)
        {
            GameCHelpButton.OnPressed -= CHelpButtonPressed;
            GameCHelpButton.OnPressed += CHelpButtonPressed;
            GameCHelpButton.Pressed = UIHelper?.IsOpen ?? false;

            if (_hasUnreadCHelp)
            {
                UnreadCHelpReceived();
            }
            else
            {
                UnreadCHelpRead();
            }
        }
    }

    public void OnStateExited(GameplayState state)
    {
        if (GameCHelpButton != null)
            GameCHelpButton.OnPressed -= CHelpButtonPressed;
    }

    public void OnStateEntered(LobbyState state)
    {
    }

    public void OnStateExited(LobbyState state)
    {
    }
}

// please kill all this indirection
public interface ICHelpUIHandler : IDisposable
{
    bool IsCurator { get; }
    bool IsOpen { get; }
    void Receive(CwoinkTextMessage message);
    void Close();
    void Open(NetUserId netUserId, bool relayActive);
    void ToggleWindow();
    void DiscordRelayChanged(bool active);
    void PeopleTypingUpdated(CwoinkPlayerTypingUpdated args);
    event Action OnClose;
    event Action OnOpen;
    Action<NetUserId, string, bool, bool>? SendMessageAction { get; set; }
    event Action<NetUserId, string>? InputTextChanged;
}

public sealed class CuratorCHelpUIHandler(NetUserId owner) : ICHelpUIHandler
{
    private readonly NetUserId _ownerId = owner;
    private readonly Dictionary<NetUserId, CwoinkPanel> _activePanelMap = new();
    public bool IsCurator => true;
    public bool IsOpen => Window is { Disposed: false, IsOpen: true } || ClydeWindow is { IsDisposed: false };
    public bool EverOpened;

    public CwoinkWindow? Window;
    public WindowRoot? WindowRoot;
    public IClydeWindow? ClydeWindow;
    public CwoinkControl? Control;

    public void Receive(CwoinkTextMessage message)
    {
        var panel = EnsurePanel(message.UserId);
        panel.ReceiveLine(message);
        Control?.OnCwoink();
    }

    private void OpenWindow()
    {
        if (Window == null)
            return;

        if (EverOpened)
            Window.Open();
        else
            Window.OpenCentered();
    }

    public void Close()
    {
        Window?.Close();

        // popped-out window is being closed
        if (ClydeWindow != null)
        {
            ClydeWindow.RequestClosed -= OnRequestClosed;
            ClydeWindow.Dispose();
            // need to dispose control cause we cant reattach it directly back to the window
            // but orphan panels first so -they- can get readded when the window is opened again
            if (Control != null)
            {
                foreach (var (_, panel) in _activePanelMap)
                {
                    panel.Orphan();
                }
                Control?.Dispose();
            }
            // window wont be closed here so we will invoke ourselves
            OnClose?.Invoke();
        }
    }

    public void ToggleWindow()
    {
        EnsurePanel(_ownerId);

        if (IsOpen)
            Close();
        else
            OpenWindow();
    }

    public void DiscordRelayChanged(bool active)
    {
    }

    public void PeopleTypingUpdated(CwoinkPlayerTypingUpdated args)
    {
        if (_activePanelMap.TryGetValue(args.Channel, out var panel))
            panel.UpdatePlayerTyping(args.PlayerName, args.Typing);
    }

    public event Action? OnClose;
    public event Action? OnOpen;
    public Action<NetUserId, string, bool, bool>? SendMessageAction { get; set; }
    public event Action<NetUserId, string>? InputTextChanged;

    public void Open(NetUserId channelId, bool relayActive)
    {
        SelectChannel(channelId);
        OpenWindow();
    }

    public void OnRequestClosed(WindowRequestClosedEventArgs args)
    {
        Close();
    }

    private void EnsureControl()
    {
        if (Control is { Disposed: false })
            return;

        Window = new CwoinkWindow();
        Control = Window.Cwoink;
        Window.OnClose += () => { OnClose?.Invoke(); };
        Window.OnOpen += () =>
        {
            OnOpen?.Invoke();
            EverOpened = true;
        };

        // need to readd any unattached panels..
        foreach (var (_, panel) in _activePanelMap)
        {
            if (!Control!.CwoinkArea.Children.Contains(panel))
            {
                Control!.CwoinkArea.AddChild(panel);
            }
            panel.Visible = false;
        }
    }

    public void HideAllPanels()
    {
        foreach (var panel in _activePanelMap.Values)
        {
            panel.Visible = false;
        }
    }

    public CwoinkPanel EnsurePanel(NetUserId channelId)
    {
        EnsureControl();

        if (_activePanelMap.TryGetValue(channelId, out var existingPanel))
            return existingPanel;

        _activePanelMap[channelId] = existingPanel = new CwoinkPanel(text => SendMessageAction?.Invoke(channelId, text, Window?.Cwoink.PlaySound.Pressed ?? true, Window?.Cwoink.CuratorOnly.Pressed ?? false));
        existingPanel.InputTextChanged += text => InputTextChanged?.Invoke(channelId, text);
        existingPanel.Visible = false;
        if (!Control!.CwoinkArea.Children.Contains(existingPanel))
            Control.CwoinkArea.AddChild(existingPanel);

        return existingPanel;
    }

    public bool TryGetChannel(NetUserId ch, [NotNullWhen(true)] out CwoinkPanel? bp) => _activePanelMap.TryGetValue(ch, out bp);

    private void SelectChannel(NetUserId uid)
    {
        EnsurePanel(uid);
        Control!.SelectChannel(uid);
    }

    public void Dispose()
    {
        Window?.Dispose();
        Window = null;
        Control = null;
        _activePanelMap.Clear();
        EverOpened = false;
    }
}

public sealed class UserCHelpUIHandler(NetUserId owner) : ICHelpUIHandler
{
    public bool IsCurator => false;
    public bool IsOpen => _window is { Disposed: false, IsOpen: true };
    private DefaultWindow? _window;
    private CwoinkPanel? _chatPanel;
    private bool _discordRelayActive;

    public void Receive(CwoinkTextMessage message)
    {
        DebugTools.Assert(message.UserId == owner);
        EnsureInit(_discordRelayActive);
        _chatPanel!.ReceiveLine(message);
        _window!.OpenCentered();
    }

    public void Close()
    {
        _window?.Close();
    }

    public void ToggleWindow()
    {
        EnsureInit(_discordRelayActive);
        if (_window!.IsOpen)
        {
            _window.Close();
        }
        else
        {
            _window.OpenCentered();
        }
    }

    public void DiscordRelayChanged(bool active)
    {
        _discordRelayActive = active;

        if (_chatPanel != null)
        {
            _chatPanel.RelayedToDiscordLabel.Visible = active;
        }
    }

    public void PeopleTypingUpdated(CwoinkPlayerTypingUpdated args)
    {
    }

    public event Action? OnClose;
    public event Action? OnOpen;
    public Action<NetUserId, string, bool, bool>? SendMessageAction { get; set; }
    public event Action<NetUserId, string>? InputTextChanged;

    public void Open(NetUserId channelId, bool relayActive)
    {
        EnsureInit(relayActive);
        _window!.OpenCentered();
    }

    private void EnsureInit(bool relayActive)
    {
        if (_window is { Disposed: false })
            return;
        _chatPanel = new CwoinkPanel(text => SendMessageAction?.Invoke(owner, text, true, false));
        _chatPanel.InputTextChanged += text => InputTextChanged?.Invoke(owner, text);
        _chatPanel.RelayedToDiscordLabel.Visible = relayActive;
        _window = new DefaultWindow()
        {
            TitleClass = "windowTitleAlert",
            HeaderClass = "windowHeaderCurator",
            Title = Loc.GetString("cwoink-user-title"),
            MinSize = new Vector2(450, 400),
        };
        _window.OnClose += () => { OnClose?.Invoke(); };
        _window.OnOpen += () => { OnOpen?.Invoke(); };
        _window.Contents.AddChild(_chatPanel);

        var introText = Loc.GetString("cwoink-system-introductory-message");
        var introMessage = new CwoinkTextMessage(owner, SharedCwoinkSystem.SystemUserId, introText);
        Receive(introMessage);
    }

    public void Dispose()
    {
        _window?.Dispose();
        _window = null;
        _chatPanel = null;
    }
}
