using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Content.Server.Administration;
using Content.Server.Administration.Managers;
using Content.Server.Afk;
using Content.Server.Afk.Events;
using Content.Server.Database;
using Content.Server.Discord;
using Content.Server.GameTicking;
using Content.Server.Players.RateLimiting;
using Content.Server.Preferences.Managers;
using Content.Shared._DV.CCVars;
using Content.Shared._DV.Curation;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Content.Shared.Database;
using Content.Shared.GameTicking;
using Content.Shared.Mind;
using Content.Shared.Players.RateLimiting;
using Robust.Server.Player;
using Robust.Shared;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._DV.Curation.Systems;

public sealed partial class CwoinkSystem : SharedCwoinkSystem
{
    private const string RateLimitKey = "CuratorHelp";

    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly IBanManager _banManager = default!; // Starlight
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerLocator _playerLocator = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly SharedMindSystem _minds = default!;
    [Dependency] private readonly IAfkManager _afkManager = default!;
    [Dependency] private readonly IServerDbManager _dbManager = default!;
    [Dependency] private readonly PlayerRateLimitManager _rateLimit = default!;
    [Dependency] private readonly IServerPreferencesManager _preferencesManager = default!; // Frontier

    [GeneratedRegex(@"^https://(?:(?:canary|ptb)\.)?discord\.com/api/webhooks/(\d+)/((?!.*/).*)$")] // Frontier: support alt discords
    private static partial Regex DiscordRegex();

    private string _webhookUrl = string.Empty;
    private WebhookData? _webhookData;

    private readonly HttpClient _httpClient = new();

    private string _footerIconUrl = string.Empty;
    private string _avatarUrl = string.Empty;
    private string _serverName = string.Empty;

    private readonly Dictionary<NetUserId, DiscordRelayInteraction> _relayMessages = [];

    private Dictionary<NetUserId, string> _oldMessageIds = [];
    private readonly Dictionary<NetUserId, Queue<DiscordRelayedData>> _messageQueues = [];
    private readonly HashSet<NetUserId> _processingChannels = [];
    private readonly Dictionary<NetUserId, (TimeSpan Timestamp, bool Typing)> _typingUpdateTimestamps = [];
    private string _overrideClientName = string.Empty;

    // Max embed description length is 4096, according to https://discord.com/developers/docs/resources/channel#embed-object-embed-limits
    // Keep small margin, just to be safe
    private const ushort DescriptionMax = 4000;

    // Maximum length a message can be before it is cut off
    // Should be shorter than DescriptionMax
    private const ushort MessageLengthCap = 3000;

    private readonly TimeSpan _messageCooldown = TimeSpan.FromSeconds(2);

    private readonly Queue<(NetUserId Channel, string Text, TimeSpan Timestamp)> _recentMessages = new();
    private const int MaxRecentMessages = 10;

    // Text to be used to cut off messages that are too long. Should be shorter than MessageLengthCap
    private const string TooLongText = "... **(too long)**";

    private int _maxAdditionalChars;
    private readonly Dictionary<NetUserId, DateTime> _activeConversations = [];

    // CHelp config settings
    private bool _useAdminOOCColorInBwoinks = true;
    private bool _useDiscordRoleColor = false;
    private bool _useDiscordRoleName = false;
    private string _discordReplyPrefix = "(DISCORD) ";
    private string _adminBwoinkColor = "#9552cc";
    private string _discordReplyColor = string.Empty;

    // CHelp admin cache
    private readonly HashSet<INetChannel> _activeCurators = [];
    private readonly HashSet<INetChannel> _nonAfkCurators = [];

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_config, CVars.GameHostName, OnServerNameChanged, true);

        Subs.CVar(_config, DCCVars.DiscordCHelpWebhook, OnWebhookChanged, true);
        Subs.CVar(_config, DCCVars.DiscordCHelpFooterIcon, OnFooterIconChanged, true);
        Subs.CVar(_config, DCCVars.DiscordCHelpAvatar, OnAvatarChanged, true);

        Subs.CVar(_config, DCCVars.CuratorChelpOverrideClientName, OnOverrideChanged, true);
        Subs.CVar(_config, DCCVars.UseAdminOOCColorInCwoinks, OnUseAdminOOCColorInBwoinksChanged, true);
        Subs.CVar(_config, DCCVars.UseDiscordRoleColorInCwoinks, OnUseDiscordRoleColorChanged, true);
        Subs.CVar(_config, DCCVars.UseDiscordRoleNameInCwoinks, OnUseDiscordRoleNameChanged, true);
        Subs.CVar(_config, DCCVars.DiscordCwoinkReplyPrefix, OnDiscordReplyPrefixChanged, true);
        Subs.CVar(_config, DCCVars.CuratorCwoinkColor, OnAdminBwoinkColorChanged, true);
        Subs.CVar(_config, DCCVars.DiscordCwoinkReplyColor, OnDiscordReplyColorChanged, true);

        var defaultParams = new CHelpMessageParams(
            string.Empty,
            string.Empty,
            true,
            _gameTicker.RoundDuration().ToString("hh\\:mm\\:ss"),
            _gameTicker.RunLevel,
            playedSound: false
        );
        _maxAdditionalChars = GenerateAHelpMessage(defaultParams, _discordReplyPrefix).Message.Length;
        _playerManager.PlayerStatusChanged += OnPlayerStatusChanged;

        SubscribeLocalEvent<GameRunLevelChangedEvent>(OnGameRunLevelChanged);
        SubscribeNetworkEvent<CwoinkClientTypingUpdated>(OnClientTypingUpdated);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(_ => _activeConversations.Clear());
        SubscribeLocalEvent<AFKEvent>(OnAFK);
        SubscribeLocalEvent<UnAFKEvent>(OnUnAFK);

        _adminManager.OnPermsChanged += OnAdminPermsChanged;

        _rateLimit.Register(
            RateLimitKey,
            new RateLimitRegistration(CCVars.AhelpRateLimitPeriod,
                CCVars.AhelpRateLimitCount,
                PlayerRateLimitedAction)
        );

        ResetCache();
    }

    private void ResetCache()
    {
        _activeCurators.Clear();
        _nonAfkCurators.Clear();

        foreach (var admin in _adminManager.ActiveAdmins)
        {
            if (!(_adminManager.GetAdminData(admin)?.HasFlag(AdminFlags.CuratorHelp) ?? false))
                continue;

            _activeCurators.Add(admin.Channel);

            if (_afkManager.IsAfk(admin))
                continue;

            _nonAfkCurators.Add(admin.Channel);
        }
    }

    private void OnAFK(ref AFKEvent ev)
    {
        _nonAfkCurators.Remove(ev.Session.Channel);
    }

    private void OnUnAFK(ref UnAFKEvent ev)
    {
        if (_activeCurators.Contains(ev.Session.Channel))
            _nonAfkCurators.Add(ev.Session.Channel);
    }

    private void OnAdminPermsChanged(AdminPermsChangedEventArgs args)
    {
        if (((args.Flags ?? 0) & AdminFlags.CuratorHelp) != 0)
            _activeCurators.Add(args.Player.Channel);
        else
        {
            _activeCurators.Remove(args.Player.Channel);
            _nonAfkCurators.Remove(args.Player.Channel);
        }
    }

    private void OnDiscordReplyColorChanged(string newValue)
    {
        _discordReplyColor = newValue;
    }

    private void OnAdminBwoinkColorChanged(string newValue)
    {
        _adminBwoinkColor = newValue;
    }

    private void OnDiscordReplyPrefixChanged(string newValue)
    {
        var defaultParams = new CHelpMessageParams(
            string.Empty,
            string.Empty,
            true,
            _gameTicker.RoundDuration().ToString("hh\\:mm\\:ss"),
            _gameTicker.RunLevel,
            playedSound: false
        );

        _discordReplyPrefix = newValue;
        _maxAdditionalChars = GenerateAHelpMessage(defaultParams, _discordReplyPrefix).Message.Length;
    }

    private void OnUseDiscordRoleNameChanged(bool newValue)
    {
        _useDiscordRoleName = newValue;
    }

    private void OnUseDiscordRoleColorChanged(bool newValue)
    {
        _useDiscordRoleColor = newValue;
    }

    private void OnUseAdminOOCColorInBwoinksChanged(bool newValue)
    {
        _useAdminOOCColorInBwoinks = newValue;
    }

    private void PlayerRateLimitedAction(ICommonSession obj)
    {
        RaiseNetworkEvent(
            new CwoinkTextMessage(obj.UserId, default, Loc.GetString("bwoink-system-rate-limited"), playSound: false),
            obj.Channel);
    }

    private void OnOverrideChanged(string obj)
    {
        _overrideClientName = obj;
    }

    private async void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        if (e.NewStatus == SessionStatus.Disconnected)
        {
            if (_activeConversations.TryGetValue(e.Session.UserId, out var lastMessageTime))
            {
                var timeSinceLastMessage = DateTime.Now - lastMessageTime;
                if (timeSinceLastMessage > TimeSpan.FromMinutes(5))
                {
                    _activeConversations.Remove(e.Session.UserId);
                    return; // Do not send disconnect message if timeout exceeded
                }
            }

            // Check if the user has been banned
            var ban = await _dbManager.GetServerBanAsync(null, e.Session.UserId, null, null);
            if (ban != null)
            {
                _activeConversations.Remove(e.Session.UserId);
                return;
            }
        }

        // Notify all admins if a player disconnects or reconnects
        var message = e.NewStatus switch
        {
            SessionStatus.Connected => Loc.GetString("bwoink-system-player-reconnecting"),
            SessionStatus.Disconnected => Loc.GetString("bwoink-system-player-disconnecting"),
            _ => null
        };

        if (message != null)
        {
            var statusType = e.NewStatus == SessionStatus.Connected
                ? PlayerStatusType.Connected
                : PlayerStatusType.Disconnected;
            NotifyAdmins(e.Session, message, statusType);
        }

        if (e.NewStatus != SessionStatus.InGame)
            return;

        RaiseNetworkEvent(new CwoinkDiscordRelayUpdated(!string.IsNullOrWhiteSpace(_webhookUrl)), e.Session);
    }

    private void NotifyAdmins(ICommonSession session, string message, PlayerStatusType statusType)
    {
        if (!_activeConversations.ContainsKey(session.UserId))
        {
            // If the user is not part of an active conversation, do not notify admins.
            return;
        }

        // Get the current timestamp
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var roundTime = _gameTicker.RoundDuration().ToString("hh\\:mm\\:ss");

        // Determine the icon based on the status type
        var icon = statusType switch
        {
            PlayerStatusType.Connected => ":green_circle:",
            PlayerStatusType.Disconnected => ":red_circle:",
            PlayerStatusType.Banned => ":red_circle:",
            _ => ":question:"
        };

        // Create the message parameters for Discord
        var messageParams = new CHelpMessageParams(
            session.Name,
            message,
            true,
            roundTime,
            _gameTicker.RunLevel,
            playedSound: true,
            icon: icon
        );

        // Create the message for in-game with username
        var color = statusType switch
        {
            PlayerStatusType.Connected => Color.Green.ToHex(),
            PlayerStatusType.Disconnected => Color.Yellow.ToHex(),
            PlayerStatusType.Banned => Color.Yellow.ToHex(),
            _ => Color.Gray.ToHex(),
        };
        var inGameMessage = $"[color={color}]{session.Name} {message}[/color]";

        var cwoinkMessage = new CwoinkTextMessage(
            userId: session.UserId,
            trueSender: SystemUserId,
            text: inGameMessage,
            sentAt: DateTime.Now,
            playSound: false
        );

        var admins = _activeCurators;
        foreach (var admin in admins)
        {
            RaiseNetworkEvent(cwoinkMessage, admin);
        }

        // Enqueue the message for Discord relay
        if (_webhookUrl != string.Empty)
        {
            var queue = _messageQueues.GetOrNew(session.UserId);
            var escapedText = FormattedMessage.EscapeText(message);
            messageParams.Message = escapedText;
            var discordMessage = GenerateAHelpMessage(messageParams, _discordReplyPrefix);
            queue.Enqueue(discordMessage);
        }
    }

    private void OnGameRunLevelChanged(GameRunLevelChangedEvent args)
    {
        // Don't make a new embed if we
        // 1. were in the lobby just now, and
        // 2. are not entering the lobby or directly into a new round.
        if (args.Old is GameRunLevel.PreRoundLobby ||
            args.New is not (GameRunLevel.PreRoundLobby or GameRunLevel.InRound))
        {
            return;
        }

        // Store the Discord message IDs of the previous round
        _oldMessageIds.Clear();
        foreach (var (user, interaction) in _relayMessages)
        {
            var id = interaction.Id;
            if (id == null)
                return;

            _oldMessageIds[user] = id;
        }

        _relayMessages.Clear();
    }

    private void OnClientTypingUpdated(CwoinkClientTypingUpdated msg, EntitySessionEventArgs args)
    {
        if (_typingUpdateTimestamps.TryGetValue(args.SenderSession.UserId, out var tuple) &&
            tuple.Typing == msg.Typing &&
            _timing.RealTime - tuple.Timestamp < TimeSpan.FromSeconds(1))
        {
            return;
        }

        _typingUpdateTimestamps[args.SenderSession.UserId] = (_timing.RealTime, msg.Typing);

        // Non-admins can only ever type on their own ahelp, guard against fake messages
        var isAdmin = _adminManager.GetAdminData(args.SenderSession)?.HasFlag(AdminFlags.CuratorHelp) ?? false;
        var channel = isAdmin ? msg.Channel : args.SenderSession.UserId;
        var update = new CwoinkPlayerTypingUpdated(channel, args.SenderSession.Name, msg.Typing);

        foreach (var admin in _activeCurators)
        {
            if (admin.UserId == args.SenderSession.UserId)
                continue;

            RaiseNetworkEvent(update, admin);
        }
    }

    private void OnServerNameChanged(string obj)
    {
        _serverName = obj;
    }

    private async void OnWebhookChanged(string url)
    {
        _webhookUrl = url;

        RaiseNetworkEvent(new CwoinkDiscordRelayUpdated(!string.IsNullOrWhiteSpace(url)));

        if (url == string.Empty)
            return;

        // Basic sanity check and capturing webhook ID and token
        var match = DiscordRegex().Match(url);

        if (!match.Success)
        {
            // TODO: Ideally, CVar validation during setting should be better integrated
            Log.Warning("Webhook URL does not appear to be valid. Using anyways...");
            await GetWebhookData(url); // Frontier - Support for Custom URLS, we still want to see if theres Webhook data available
            return;
        }

        if (match.Groups.Count <= 2)
        {
            Log.Error("Could not get webhook ID or token.");
            return;
        }

        // Fire and forget
        await GetWebhookData(url); // Frontier - Support for Custom URLS
    }

    private async Task<WebhookData?> GetWebhookData(string url) // Frontier - Support for Custom URLS
    {
        var response = await _httpClient.GetAsync(url); // Frontier

        var content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            Log.Error(
                $"Discord returned bad status code when trying to get webhook data (perhaps the webhook URL is invalid?): {response.StatusCode}\nResponse: {content}");
            return null;
        }

        return JsonSerializer.Deserialize<WebhookData>(content);
    }

    private void OnFooterIconChanged(string url)
    {
        _footerIconUrl = url;
    }

    private void OnAvatarChanged(string url)
    {
        _avatarUrl = url;
    }

    private async void ProcessQueue(NetUserId userId, Queue<DiscordRelayedData> messages)
    {
        // Whether an embed already exists for this player
        var exists = _relayMessages.TryGetValue(userId, out var existingEmbed);

        // Whether the message will become too long after adding these new messages
        var tooLong = exists && messages.Sum(msg => Math.Min(msg.Message.Length, MessageLengthCap) + "\n".Length)
            + existingEmbed?.Description.Length > DescriptionMax;

        // If there is no existing embed, or it is getting too long, we create a new embed
        if (!exists || tooLong)
        {
            var lookup = await _playerLocator.LookupIdAsync(userId);

            if (lookup == null)
            {
                Log.Error(
                    $"Unable to find player for NetUserId {userId} when sending discord webhook.");
                _relayMessages.Remove(userId);
                return;
            }

            var linkToPrevious = string.Empty;

            // If we have all the data required, we can link to the embed of the previous round or embed that was too long
            if (_webhookData is { GuildId: { } guildId, ChannelId: { } channelId })
            {
                if (tooLong && existingEmbed?.Id != null)
                {
                    linkToPrevious =
                        $"**[Go to previous embed of this round](https://discord.com/channels/{guildId}/{channelId}/{existingEmbed.Id})**\n";
                }
                else if (_oldMessageIds.TryGetValue(userId, out var id) && !string.IsNullOrEmpty(id))
                {
                    linkToPrevious =
                        $"**[Go to last round's conversation with this player](https://discord.com/channels/{guildId}/{channelId}/{id})**\n";
                }
            }

            var characterName = _minds.GetCharacterName(userId);
            existingEmbed = new DiscordRelayInteraction()
            {
                Id = null,
                CharacterName = characterName,
                Description = linkToPrevious,
                Username = lookup.Username,
                LastRunLevel = _gameTicker.RunLevel,
            };

            _relayMessages[userId] = existingEmbed;
        }

        // Previous message was in another RunLevel, so show that in the embed
        if (existingEmbed!.LastRunLevel != _gameTicker.RunLevel)
        {
            existingEmbed.Description += _gameTicker.RunLevel switch
            {
                GameRunLevel.PreRoundLobby => "\n\n:arrow_forward: _**Pre-round lobby started**_\n",
                GameRunLevel.InRound => "\n\n:arrow_forward: _**Round started**_\n",
                GameRunLevel.PostRound => "\n\n:stop_button: _**Post-round started**_\n",
                _ => throw new ArgumentOutOfRangeException(nameof(_gameTicker.RunLevel),
                    $"{_gameTicker.RunLevel} was not matched."),
            };

            existingEmbed.LastRunLevel = _gameTicker.RunLevel;
        }

        // Add available messages to the embed description
        while (messages.TryDequeue(out var message))
        {
            string text;

            // In case someone thinks they're funny
            if (message.Message.Length > MessageLengthCap)
                text = message.Message[..(MessageLengthCap - TooLongText.Length)] + TooLongText;
            else
                text = message.Message;

            existingEmbed.Description += $"\n{text}";
        }

        var payload = GeneratePayload(existingEmbed.Description,
            existingEmbed.Username,
            userId.UserId, // Frontier, this is used to identify the players in the webhook
            existingEmbed.CharacterName);

        // If there is no existing embed, create a new one
        // Otherwise patch (edit) it
        if (existingEmbed.Id == null)
        {
            var request = await _httpClient.PostAsync($"{_webhookUrl}?wait=true",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

            var content = await request.Content.ReadAsStringAsync();
            if (!request.IsSuccessStatusCode)
            {
                Log.Error(
                    $"Discord returned bad status code when posting message (perhaps the message is too long?): {request.StatusCode}\nResponse: {content}");
                _relayMessages.Remove(userId);
                return;
            }

            var id = JsonNode.Parse(content)?["id"];
            if (id == null)
            {
                Log.Error(
                    $"Could not find id in json-content returned from discord webhook: {content}");
                _relayMessages.Remove(userId);
                return;
            }

            existingEmbed.Id = id.ToString();
        }
        else
        {
            var request = await _httpClient.PatchAsync($"{_webhookUrl}/messages/{existingEmbed.Id}",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

            if (!request.IsSuccessStatusCode)
            {
                var content = await request.Content.ReadAsStringAsync();
                Log.Error(
                    $"Discord returned bad status code when patching message (perhaps the message is too long?): {request.StatusCode}\nResponse: {content}");
                _relayMessages.Remove(userId);
                return;
            }
        }

        _relayMessages[userId] = existingEmbed;

        _processingChannels.Remove(userId);
    }

    private WebhookPayload GeneratePayload(string messages, string username, Guid userId, string? characterName = null) // Frontier: added Guid
    {
        // Add character name
        if (characterName != null)
            username += $" ({characterName})";

        // If no admins are online, set embed color to red. Otherwise green
        var color = _nonAfkCurators.Count > 0 ? 0x41F097 : 0xFF0000;

        // Limit server name to 1500 characters, in case someone tries to be a little funny
        var serverName = _serverName[..Math.Min(_serverName.Length, 1500)];

        var round = _gameTicker.RunLevel switch
        {
            GameRunLevel.PreRoundLobby => _gameTicker.RoundId == 0
                ? "pre-round lobby after server restart" // first round after server restart has ID == 0
                : $"pre-round lobby for round {_gameTicker.RoundId + 1}",
            GameRunLevel.InRound => $"round {_gameTicker.RoundId}",
            GameRunLevel.PostRound => $"post-round {_gameTicker.RoundId}",
            _ => throw new ArgumentOutOfRangeException(nameof(_gameTicker.RunLevel),
                $"{_gameTicker.RunLevel} was not matched."),
        };

        return new WebhookPayload
        {
            Username = username,
            UserID = userId, // Frontier, this is used to identify the players in the webhook
            AvatarUrl = string.IsNullOrWhiteSpace(_avatarUrl) ? null : _avatarUrl,
            Embeds =
            [
                new()
                {
                    Description = messages,
                    Color = color,
                    Footer = new WebhookEmbedFooter
                    {
                        Text = $"{serverName} ({round})",
                        IconUrl = string.IsNullOrWhiteSpace(_footerIconUrl) ? null : _footerIconUrl
                    },
                }
            ]
        };
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var userId in _messageQueues.Keys.ToArray())
        {
            if (_processingChannels.Contains(userId))
                continue;

            var queue = _messageQueues[userId];
            _messageQueues.Remove(userId);
            if (queue.Count == 0)
                continue;

            _processingChannels.Add(userId);

            ProcessQueue(userId, queue);
        }
    }

    protected override void OnCwoinkTextMessage(CwoinkTextMessage message, EntitySessionEventArgs eventArgs)
    {
        base.OnCwoinkTextMessage(message, eventArgs);

        var senderSession = eventArgs.SenderSession;

        // TODO: Sanitize text?
        // Confirm that this person is actually allowed to send a message here.
        var personalChannel = senderSession.UserId == message.UserId;
        var senderAdmin = _adminManager.GetAdminData(senderSession);
        var senderAHelpAdmin = senderAdmin?.HasFlag(AdminFlags.CuratorHelp) ?? false;
        var authorized = personalChannel && !message.AdminOnly || senderAHelpAdmin;
        if (!authorized)
        {
            // Unauthorized cwoink (log?)
            return;
        }

        // Begin Starlight Changes
        var currentTime = _timing.RealTime;

        if (IsOnCooldown(message.UserId, currentTime))
            return;

        if (IsSpam(message.UserId, message.Text))
            _banManager.CreateServerBan(senderSession.UserId, senderSession.Name, null, null, null, 0, NoteSeverity.High, "Automatic AHELP Antispam system Ban, If this ban is wrong, file an appeal.");

        AddToRecentMessages(message.UserId, message.Text, currentTime);
        // End Starlight Changes

        if (_rateLimit.CountAction(eventArgs.SenderSession, RateLimitKey) != RateLimitStatus.Allowed)
            return;

        var cwoinkParams = new CwoinkParams(message,
            eventArgs.SenderSession.UserId,
            senderAdmin,
            eventArgs.SenderSession.Name,
            eventArgs.SenderSession.Channel,
            false,
            true,
            false);
        OnCwoinkInternal(cwoinkParams);
    }

    /// <summary>
    /// Sends a cwoink. Common to both internal messages (sent via the ahelp or admin interface) and webhook messages (sent through the webhook, e.g. via Discord)
    /// </summary>
    /// <param name="cwoinkParams">The parameters of the message being sent.</param>
    private void OnCwoinkInternal(CwoinkParams cwoinkParams)
    {
        var fromWebhook = cwoinkParams.FromWebhook;
        var message = cwoinkParams.Message;
        var roleColor = cwoinkParams.RoleColor;
        var roleName = cwoinkParams.RoleName;
        var senderAdmin = cwoinkParams.SenderAdmin;
        var senderChannel = cwoinkParams.SenderChannel;
        var senderId = cwoinkParams.SenderId;
        var senderName = cwoinkParams.SenderName;
        var userOnly = cwoinkParams.UserOnly;
        var sendWebhook = cwoinkParams.SendWebhook;

        _activeConversations[message.UserId] = DateTime.Now;

        var escapedText = FormattedMessage.EscapeText(message.Text);
        var adminColor = _adminBwoinkColor;
        var adminPrefix = "";
        var cwoinkText = $"{senderName}";

        //Getting an administrator position
        if (_config.GetCVar(CCVars.AhelpAdminPrefix))
        {
            if (senderAdmin is not null && senderAdmin.Title is not null)
                adminPrefix = $"[bold]\\[{senderAdmin.Title}\\][/bold] ";

            if (_useDiscordRoleName && roleName is not null)
                adminPrefix = $"[bold]\\[{roleName}\\][/bold] ";
        }

        if (!fromWebhook
            && _useAdminOOCColorInBwoinks
            && senderAdmin is not null)
        {
            var prefs = _preferencesManager.GetPreferences(senderId);
            adminColor = prefs.AdminOOCColor.ToHex();
        }

        // If role color is enabled and exists, use it, otherwise use the discord reply color
        if (_discordReplyColor != string.Empty && fromWebhook)
            adminColor = _discordReplyColor;

        if (_useDiscordRoleColor && roleColor is not null)
            adminColor = roleColor;

        if (senderAdmin is not null && (fromWebhook || senderAdmin.HasFlag(AdminFlags.CuratorHelp) || senderAdmin.HasFlag(AdminFlags.Adminhelp)))
        {
            cwoinkText = $"[color={adminColor}]{adminPrefix}{senderName}[/color]";
        }

        if (fromWebhook)
            cwoinkText = $"{_discordReplyPrefix}{cwoinkText}";

        cwoinkText = $"{(message.AdminOnly ? Loc.GetString("cwoink-message-curator-only") : !message.PlaySound ? Loc.GetString("cwoink-message-silent") : "")} {cwoinkText}: {escapedText}";

        // If it's not an admin / admin chooses to keep the sound and message is not an admin only message, then play it.
        var playSound = (senderAdmin == null || message.PlaySound) && !message.AdminOnly;
        var msg = new CwoinkTextMessage(message.UserId, senderId, cwoinkText, playSound: playSound, adminOnly: message.AdminOnly);

        LogCwoink(msg);

        var admins = _activeCurators;

        // Notify all admins
        if (!userOnly)
        {
            foreach (var channel in admins)
            {
                RaiseNetworkEvent(msg, channel);
            }
        }

        var adminPrefixWebhook = string.Empty;

        if (_config.GetCVar(CCVars.AhelpAdminPrefixWebhook) && senderAdmin is not null && senderAdmin.Title is not null)
        {
            adminPrefixWebhook = $"[bold]\\[{senderAdmin.Title}\\][/bold] ";
        }

        // Notify player
        if (_playerManager.TryGetSessionById(message.UserId, out var session) && !message.AdminOnly)
        {
            if (!admins.Contains(session.Channel))
            {
                // If _overrideClientName is set, we generate a new message with the override name. The admins name will still be the original name for the webhooks.
                if (_overrideClientName != string.Empty)
                {
                    string overrideMsgText;

                    if (senderAdmin is not null && senderAdmin.HasFlag(AdminFlags.CuratorHelp))
                        overrideMsgText = $"[color={_adminBwoinkColor}]{adminPrefixWebhook}{_overrideClientName}[/color]";
                    else
                        overrideMsgText = $"{senderName}"; // Not an admin, name is not overridden.

                    if (fromWebhook)
                        overrideMsgText = $"{_discordReplyPrefix}{overrideMsgText}";

                    overrideMsgText = $"{(message.PlaySound ? "" : "(S) ")}{overrideMsgText}: {escapedText}";

                    RaiseNetworkEvent(new CwoinkTextMessage(message.UserId,
                            senderId,
                            overrideMsgText,
                            playSound: playSound),
                        session.Channel);
                }
                else
                    RaiseNetworkEvent(msg, session.Channel);
            }
        }

        var sendsWebhook = _webhookUrl != string.Empty;
        if (sendsWebhook && sendWebhook)
        {
            if (!_messageQueues.ContainsKey(msg.UserId))
                _messageQueues[msg.UserId] = new Queue<DiscordRelayedData>();

            var str = message.Text;
            var unameLength = senderName.Length;

            if (unameLength + str.Length + _maxAdditionalChars > DescriptionMax)
            {
                str = str[..(DescriptionMax - _maxAdditionalChars - unameLength)];
            }

            var nonAfkAdmins = _nonAfkCurators;
            var messageParams = new CHelpMessageParams(
                senderName,
                str,
                senderId != message.UserId,
                _gameTicker.RoundDuration().ToString("hh\\:mm\\:ss"),
                _gameTicker.RunLevel,
                playedSound: playSound,
                isDiscord: fromWebhook, // DeltaV
                curatorOnly: message.AdminOnly,
                noReceivers: nonAfkAdmins.Count == 0
            );
            _messageQueues[msg.UserId].Enqueue(GenerateAHelpMessage(messageParams, _discordReplyPrefix));
        }

        if (admins.Count != 0 || sendsWebhook)
            return;

        // No admin online, let the player know
        if (senderChannel != null)
        {
            var systemText = Loc.GetString("cwoink-system-starmute-message-no-other-users");
            var starMuteMsg = new CwoinkTextMessage(message.UserId, SystemUserId, systemText);
            RaiseNetworkEvent(starMuteMsg, senderChannel);
        }
    }

    private DiscordRelayedData GenerateAHelpMessage(CHelpMessageParams parameters, string? discordReplyPrefix = "(DISCORD)") // DeltaV - added reply prefix
    {
        var stringbuilder = new StringBuilder();

        if (parameters.Icon != null)
            stringbuilder.Append(parameters.Icon);
        else if (parameters.IsCurator)
            stringbuilder.Append(":outbox_tray:");
        else if (parameters.NoReceivers)
            stringbuilder.Append(":sleeping:");
        else
            stringbuilder.Append(":inbox_tray:");

        if (parameters.RoundTime != string.Empty && parameters.RoundState == GameRunLevel.InRound)
            stringbuilder.Append($" **{parameters.RoundTime}**");
        if (!parameters.PlayedSound)
            stringbuilder.Append($" **{(parameters.CuratorOnly ? Loc.GetString("cwoink-message-admin-only") : Loc.GetString("cwoink-message-silent"))}**");

        if (parameters.IsDiscord) // Frontier - Discord Indicator
            stringbuilder.Append($" **{discordReplyPrefix}**");

        if (parameters.Icon == null)
            stringbuilder.Append($" **{parameters.Username}:** ");
        else
            stringbuilder.Append($" **{parameters.Username}** ");
        stringbuilder.Append(parameters.Message);

        return new DiscordRelayedData()
        {
            Receivers = !parameters.NoReceivers,
            Message = stringbuilder.ToString(),
        };
    }

    private record struct DiscordRelayedData
    {
        /// <summary>
        /// Was anyone online to receive it.
        /// </summary>
        public bool Receivers;

        /// <summary>
        /// What's the payload to send to discord.
        /// </summary>
        public string Message;
    }

    /// <summary>
    ///  Class specifically for holding information regarding existing Discord embeds
    /// </summary>
    private sealed class DiscordRelayInteraction
    {
        public string? Id;

        public string Username = string.Empty;

        public string? CharacterName;

        /// <summary>
        /// Contents for the discord message.
        /// </summary>
        public string Description = string.Empty;

        /// <summary>
        /// Run level of the last interaction. If different we'll link to the last Id.
        /// </summary>
        public GameRunLevel LastRunLevel;
    }

    private void AddToRecentMessages(NetUserId channelId, string text, TimeSpan timestamp)
    {
        _recentMessages.Enqueue((channelId, text, timestamp));

        if (_recentMessages.Count > MaxRecentMessages)
        {
            _recentMessages.Dequeue();
        }
    }

    private bool IsOnCooldown(NetUserId channelId, TimeSpan currentTime)
    {
        var lastMessage = _recentMessages
            .Where(msg => msg.Channel == channelId)
            .OrderByDescending(msg => msg.Timestamp)
            .FirstOrDefault();

        return lastMessage != default && (currentTime - lastMessage.Timestamp) < _messageCooldown;
    }

    private bool IsSpam(NetUserId channelId, string text)
    {
        var recentMessages = _recentMessages
            .Where(msg => msg.Channel == channelId)
            .OrderByDescending(msg => msg.Timestamp)
            .Take(10);

        return recentMessages.All(msg => msg.Text == text) && recentMessages.Count() >= 5;
    }

    public IEnumerable<(NetUserId Channel, string Text, TimeSpan Timestamp)> GetRecentMessages()
    {
        return _recentMessages;
    }
}

public struct CHelpMessageParams(
    string username,
    string message,
    bool isCurator,
    string roundTime,
    GameRunLevel roundState,
    bool playedSound,
    bool isDiscord = false,
    bool curatorOnly = false,
    bool noReceivers = false,
    string? icon = null)
{
    public string Username { get; set; } = username;
    public string Message { get; set; } = message;
    public bool IsCurator { get; set; } = isCurator;
    public string RoundTime { get; set; } = roundTime;
    public GameRunLevel RoundState { get; set; } = roundState;
    public bool PlayedSound { get; set; } = playedSound;
    public readonly bool CuratorOnly = curatorOnly;
    public bool NoReceivers { get; set; } = noReceivers;
    public bool IsDiscord { get; set; } = isDiscord;
    public string? Icon { get; set; } = icon;
}

public enum PlayerStatusType
{
    Connected,
    Disconnected,
    Banned,
}
