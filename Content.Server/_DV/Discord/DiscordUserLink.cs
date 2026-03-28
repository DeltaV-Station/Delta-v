using Content.Server.Database;
using Content.Server.Discord.DiscordLink;
using Robust.Server.Player;
using Robust.Shared.Network;
using System.Linq;
using System.Threading.Tasks;

namespace Content.Server._DV.Discord;
public sealed partial class DiscordUserLink : EntitySystem
{
    [Dependency] private readonly DiscordLink _discordLink = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IServerDbManager _dbManager = default!;
    [Dependency] private readonly ILogManager _logManager = default!;

    private HashSet<ActiveDiscordLink> _links = new();
    private HashSet<ulong> _readDisclaimer = new();
    private HashSet<PendingLink> _pendingLinks = new();

    private ISawmill _sawmill = default!;

    private readonly char[] CodeLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
    private const int CodeLength = 6;
    private const string DMFailMessage = "Please send the command directly to me instead of on the server.";

    public override void Initialize()
    {
        base.Initialize();
        _sawmill = _logManager.GetSawmill("userlink");

        _playerManager.PlayerStatusChanged += OnPlayerStatusChanged;
        _discordLink.RegisterCommandCallback(OnVerifyCommandRun, "verify");
        _discordLink.RegisterCommandCallback(OnUnverifyCommandRun, "unverify");
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _playerManager.PlayerStatusChanged -= OnPlayerStatusChanged;
    }

    public bool TryGameVerify(NetUserId userId, string code)
    {
        if (_pendingLinks.All(link => link.Code != code))
        {
            return false;
        }

        var pendingCode = _pendingLinks.First(link => link.Code == code);
        _pendingLinks.Remove(pendingCode);

        _links.Add(new (userId, pendingCode.DiscordUserId));
        UpdatePlayerLink(userId, pendingCode.DiscordUserId);
        return true;
    }

    private void OnVerifyCommandRun(CommandReceivedEventArgs args)
    {
        if (args.Arguments.Find(s => s.Equals("confirm")) != string.Empty && _readDisclaimer.Contains(args.Message.Author.Id))
        {
            if (!IsDirectMessage(args))
            {
                Task.Run(async () =>
                {
                    var replyMessage = await args.Message.ReplyAsync(DMFailMessage);
                    await args.Message.DeleteAsync();
                    await Task.Delay(3000);
                    await replyMessage.DeleteAsync();
                });
                return;
            }

            _readDisclaimer.Remove(args.Message.Author.Id);
            OnConfirmationReceived(args);
            return;
        }

        string replyMessage = "# Disclaimer\nBy linking your account to the game, " +
            "you understand that we are storing a reference between your discord account and your SS14 account. " +
            "If you do not wish to have that connection, please stop here.\n\nTo confirm, please type !verify confirm\n You can opt our at any time with !unverify.";

        Task.Run(async () =>
        {
            await SendDirectMessage(args.Message.Author.Id, replyMessage);
            _readDisclaimer.Add(args.Message.Author.Id);
            await args.Message.DeleteAsync();
        });
    }

    private void OnConfirmationReceived(CommandReceivedEventArgs args)
    {
        var code = StartVerify(args.Message.Author.Id);
        string message = $"On the game server, type ``verify {code}`` to verify your discord account.";

        Task.Run(async () =>
        {
            await SendDirectMessage(args.Message.Author.Id, message);
        });

    }

    private void OnUnverifyCommandRun(CommandReceivedEventArgs args)
    {
        var authorId = args.Message.Author.Id;
        var failMessage = "There was no account linked to your Discord Account.";
        var message = "Your account has been unverified, and the stored data erased.";

        if (!IsDirectMessage(args))
        {
            Task.Run(async () =>
            {
                var replyMessage = await args.Message.ReplyAsync(DMFailMessage);
                await args.Message.DeleteAsync();
                await Task.Delay(3000);
                await replyMessage.DeleteAsync();
            });
            return;
        }

        if (!_links.Any(link => link.DiscordUserId == authorId))
        {
            Task.Run(async () =>
            {
                await SendDirectMessage(authorId, failMessage);
            });
            return;
        }

        Task.Run(async () =>
        {
            await SendDirectMessage(authorId, message);
        });

        _links.RemoveWhere(link => link.DiscordUserId == authorId);
        UpdatePlayerLink(authorId, null);
    }
}
