using System.Diagnostics.CodeAnalysis;
using System.Linq; // DeltaV
using Content.Client.Administration.Systems;
using Content.Client.Stylesheets;
using Content.Client.UserInterface.Systems.Bwoink;
using Content.Shared._RMC14.CCVar;
using Content.Shared._RMC14.Mentor;
using Content.Shared.Administration; // DeltaV
using Content.Shared.Input;
using Robust.Client.Audio;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Configuration;
using Robust.Shared.Input.Binding;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Client._RMC14.Mentor;

public sealed class StaffHelpUIController : UIController, IOnSystemChanged<BwoinkSystem>
{
    [Dependency] private readonly AHelpUIController _aHelp = default!;
    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [UISystemDependency] private readonly AudioSystem? _audio = default!;
    [UISystemDependency] private readonly AdminSystem _admin = default!; // DeltaV

    private readonly Dictionary<NetUserId, List<MentorMessage>> _messages = new();
    //private readonly Dictionary<NetUserId, string> _destinationNames = new(); DeltaV - All players in chat

    private bool _isMentor;
    //private bool _canReMentor; DeltaV - Remove de/re-mentor
    private StaffHelpWindow? _staffHelpWindow;
    private MentorHelpWindow? _mentorHelpWindow;
    private MentorWindow? _mentorWindow;
    private string? _mHelpSound;
    private bool _unread;

    public override void Initialize()
    {
        _net.RegisterNetMessage<MentorStatusMsg>(OnMentorStatus);
        _net.RegisterNetMessage<MentorMessagesReceivedMsg>(OnMentorHelpReceived);
        _net.RegisterNetMessage<MentorSendMessageMsg>();
        _net.RegisterNetMessage<MentorHelpMsg>();
        //_net.RegisterNetMessage<DeMentorMsg>(); DeltaV - Remove de/re-mentor
        //_net.RegisterNetMessage<ReMentorMsg>(); DeltaV - Remove de/re-mentor
        _config.OnValueChanged(RMCCVars.RMCMentorHelpSound, v => _mHelpSound = v, true);
    }

    private void OnMentorStatus(MentorStatusMsg msg)
    {
        _isMentor = msg.IsMentor;
        //_canReMentor = msg.CanReMentor; DeltaV - Remove de/re-mentor

        if (_isMentor)
            _mentorHelpWindow?.Close();
        else
            _mentorWindow?.Close();
    }

    private void OnMentorHelpReceived(MentorMessagesReceivedMsg msg)
    {
        var other = false;
        foreach (var message in msg.Messages)
        {
            if (message.Author != _player.LocalUser)
                other = true;

            if (_isMentor &&
                _mentorWindow is not { IsOpen: true })
            {
                _unread = true;
                _aHelp.UnreadAHelpReceived();
            }

            //_destinationNames.TryAdd(message.Destination, message.DestinationName); DeltaV - All players in chat
            _messages.GetOrNew(message.Destination).Add(message);
            if (_mentorWindow is { IsOpen: true })
            {
                //MentorAddPlayerButton(message.Destination); DeltaV - All players in chat

                if (_mentorWindow.SelectedPlayer == message.Destination)
                {
                    _mentorWindow.Messages.AddMessage(CreateMessageLabel(message));
                    _mentorWindow.Messages.ScrollToBottom();
                }

                continue;
            }

            if (_mentorHelpWindow is { IsOpen: true } &&
                _player.LocalUser == message.Destination)
            {
                _mentorHelpWindow.Messages.AddMessage(CreateMessageLabel(message));
                _mentorHelpWindow.Messages.ScrollToBottom();
            }
        }

        if (other)
        {
            _audio?.PlayGlobal(_mHelpSound, Filter.Local(), false);
            _clyde.RequestWindowAttention();

            if (!_isMentor)
            {
                if (OpenWindow(ref _mentorHelpWindow, CreateMentorHelpWindow, () => _mentorHelpWindow = null))
                {
                    _mentorHelpWindow.OpenCentered();
                }
            }
        }
    }

    public void OnSystemLoaded(BwoinkSystem system)
    {
        CommandBinds.Builder
            .BindBefore(ContentKeyFunctions.OpenAHelp,
                InputCmdHandler.FromDelegate(_ => ToggleWindow()),
                typeof(AHelpUIController))
            .Register<StaffHelpUIController>();
    }

    public void OnSystemUnloaded(BwoinkSystem system)
    {
        CommandBinds.Unregister<StaffHelpUIController>();
    }

    public void ToggleWindow()
    {
        if (_staffHelpWindow != null)
        {
            _staffHelpWindow.Close();
            _staffHelpWindow = null;
            SetAHelpButtonPressed(false);
            return;
        }

        SetAHelpButtonPressed(true);
        _staffHelpWindow = new StaffHelpWindow();
        _staffHelpWindow.OnClose += () => _staffHelpWindow = null;
        _staffHelpWindow.OpenCentered();
        UIManager.ClickSound();

        if (_unread)
            _staffHelpWindow.MentorHelpButton.StyleClasses.Add(StyleNano.StyleClassButtonColorRed);

        _staffHelpWindow.AdminHelpButton.OnPressed += _ =>
        {
            _aHelp.Open();
            _staffHelpWindow.Close();
            SetAHelpButtonPressed(false);
        };

        _staffHelpWindow.MentorHelpButton.OnPressed += _ =>
        {
            SetAHelpButtonPressed(false);
            _unread = false;
            if (_isMentor)
            {
                if (OpenWindow(ref _mentorWindow, CreateMentorWindow, () => _mentorWindow = null))
                {
                    // DeltaV - Start all players in chat sorted
                    var playerList = _admin.PlayerList.ToList();
                    playerList.Sort(delegate(PlayerInfo player1, PlayerInfo player2)
                    {
                        var time1 = _messages.GetValueOrDefault(player1.SessionId);
                        var time2 = _messages.GetValueOrDefault(player2.SessionId);
                        if (time1 == null && time2 == null)
                            return 0;
                        if (time1 == null)
                            return -1;
                        if (time2 == null)
                            return 1;
                        return time1.Last().Time.CompareTo(time2.Last().Time);
                    });

                    foreach (var player in playerList)
                    {
                        MentorAddPlayerButton(player);
                    }
                    // DeltaV - End all players in chat sorted

                    _mentorWindow.OpenCentered();
                }
            }
            else
            {
                if (OpenWindow(ref _mentorHelpWindow, CreateMentorHelpWindow, () => _mentorHelpWindow = null))
                {
                    _mentorHelpWindow.OpenCentered();
                }
            }

            _staffHelpWindow.Close();
        };
    }

    private MentorHelpWindow CreateMentorHelpWindow()
    {
        var window = new MentorHelpWindow();
        //window.ReMentorButton.OnPressed += _ => _net.ClientSendMessage(new ReMentorMsg()); DeltaV - Remove de/re-mentor
        //window.ReMentorButton.Visible = _canReMentor; DeltaV - Remove de/re-mentor
        window.Chat.OnTextEntered += args =>
        {
            window.Chat.Clear();
            if (string.IsNullOrWhiteSpace(args.Text))
                return;

            var msg = new MentorHelpMsg() { Message = args.Text };
            _net.ClientSendMessage(msg);
        };

        if (_player.LocalUser is { } local && _messages.TryGetValue(local, out var messages))
        {
            foreach (var message in messages)
            {
                window.Messages.AddMessage(CreateMessageLabel(message));
                window.Messages.ScrollToBottom();
            }
        }

        return window;
    }

    private MentorWindow CreateMentorWindow()
    {
        var window = new MentorWindow();
        //window.DeMentorButton.OnPressed += _ => _net.ClientSendMessage(new DeMentorMsg()); DeltaV - Remove de/re-mentor
        window.Chat.OnTextEntered += args =>
        {
            var msg = new MentorSendMessageMsg { Message = args.Text, To = window.SelectedPlayer };
            _net.ClientSendMessage(msg);
            window.Chat.Clear();
        };

        return window;
    }

    private void MentorAddPlayerButton(PlayerInfo player)
    {
        if (_mentorWindow == null)
            return;

        /* DeltaV - Start show char name and job if possible
        if (_mentorWindow.PlayerDict.TryGetValue(player, out var button))
        {
            button.SetPositionFirst();
            return;
        }

        var playerName = player.ToString();
        if (_destinationNames.TryGetValue(player, out var destinationName))
            playerName = destinationName;
        */

        //Default show player name if they don't have a character
        var character = player.Username;
        var job = "Spectator";
        //Use Character and Job name if they exist.
        if (!string.IsNullOrWhiteSpace(player.CharacterName))
            character = player.CharacterName;
        if (!string.IsNullOrWhiteSpace(player.StartingJob))
            job = player.StartingJob;
        // DeltaV - End show char name and job if possible

        var playerButton = new Button
        {
            Text = $"{character} ({job})", // DeltaV - Show char name and job if possible
            StyleClasses = { "OpenBoth" },
        };
        playerButton.OnPressed += _ =>
        {
            if (_mentorWindow is not { IsOpen: true })
                return;

            _mentorWindow.SelectedPlayer = player.SessionId;
            _mentorWindow.Messages.Clear();
            _mentorWindow.Chat.Editable = true;
            if (!_messages.TryGetValue(player.SessionId, out var authorMessages))
                return;

            foreach (var message in authorMessages)
            {
                _mentorWindow.Messages.AddMessage(CreateMessageLabel(message));
                _mentorWindow.Messages.ScrollToBottom();
            }
        };

        _mentorWindow.Players.AddChild(playerButton);
        playerButton.SetPositionFirst();
        _mentorWindow.PlayerDict[player.SessionId] = playerButton;
    }

    private bool OpenWindow<T>([NotNullWhen(true)] ref T? window, Func<T> create, Action onClose) where T : DefaultWindow
    {
        if (window != null)
            return true;

        window = create();
        window.OnClose += onClose;
        return true;
    }

    private FormattedMessage CreateMessageLabel(MentorMessage message)
    {
        var author = message.AuthorName;
        if (message.IsMentor)
            author = $"[bold][color=red]{_player.GetPlayerData(message.Author).UserName}[/color][/bold]"; // DeltaV - Use usernames for curators

        var text = $"{message.Time:HH:mm} {author}: {FormattedMessage.EscapeText(message.Text)}";
        return FormattedMessage.FromMarkupPermissive(text);
    }

    private void SetAHelpButtonPressed(bool pressed)
    {
        if (_aHelp.GameAHelpButton != null)
            _aHelp.GameAHelpButton.Pressed = pressed;

        if (_aHelp.GameAHelpButton != null)
            _aHelp.GameAHelpButton.Pressed = pressed;
    }
}
