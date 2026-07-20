using System.Collections.ObjectModel; // DeltaV - Admin QOL
using System.Linq;
using Content.Server.Administration.Commands;
using Content.Server.Administration.Systems; // DeltaV - Admin QOL
using Content.Server.Chat.Managers;
using Content.Server.EUI;
using Content.Shared.Administration.Notes; // DeltaV - Admin QOL
using Content.Shared.Database;
using Content.Shared.Verbs;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Enums;
using Robust.Shared.Network; // DeltaV - Admin QOL
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server.Administration.Notes;

public sealed class AdminNotesSystem : EntitySystem
{
    [Dependency] private readonly IConsoleHost _console = default!;
    [Dependency] private readonly IAdminNotesManager _notes = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly EuiManager _euis = default!;
    [Dependency] private readonly AdminSystem _admin = default!; // DeltaV

    // DeltaV - watchlist cache defs START
    // For use by other systems
    // Used by lswatchlisted command to avoid querying database for every connected user every time it's invoked.
    public ReadOnlyDictionary<NetUserId, List<SharedAdminNote>> ConnectedPlayerWatchlists => _connectedPlayerWatchlists.AsReadOnly();
    private readonly Dictionary<NetUserId, List<SharedAdminNote>> _connectedPlayerWatchlists = new();
    // DeltaV - watchlist cache defs END

    public override void Initialize()
    {
        SubscribeLocalEvent<GetVerbsEvent<Verb>>(AddVerbs);
        _playerManager.PlayerStatusChanged += OnPlayerStatusChanged;

        // DeltaV - track watchlist changes START
        _notes.NoteAdded += OnNoteAdded;
        _notes.NoteModified += OnNoteModified;
        _notes.NoteDeleted += OnNoteDeleted;
        // DeltaV - track watchlist changes END
    }

    // DeltaV - track watchlist changes START
    public override void Shutdown()
    {
        base.Shutdown();

        _notes.NoteAdded -= OnNoteAdded;
        _notes.NoteModified -= OnNoteModified;
        _notes.NoteDeleted -= OnNoteDeleted;
    }

    private void OnNoteAdded(SharedAdminNote note)
    {
        if (note.NoteType != NoteType.Watchlist)
            return;

        var notes = _connectedPlayerWatchlists.GetOrNew(note.Player);
        notes.Add(note);

        if (_playerManager.TryGetSessionById(note.Player, out var session))
            _admin.UpdatePlayerList(session);
    }

    private void OnNoteModified(SharedAdminNote note)
    {
        if (note.NoteType != NoteType.Watchlist)
            return;

        var modifiedIndex = _connectedPlayerWatchlists[note.Player].FindIndex(n => n.Id == note.Id);
        if (modifiedIndex != -1)
            _connectedPlayerWatchlists[note.Player][modifiedIndex] = note;

        if (_playerManager.TryGetSessionById(note.Player, out var session))
            _admin.UpdatePlayerList(session);
    }

    private void OnNoteDeleted(SharedAdminNote note)
    {
        if (note.NoteType != NoteType.Watchlist)
            return;

        var deletedIndex = _connectedPlayerWatchlists[note.Player].FindIndex(n => n.Id == note.Id);
        if (deletedIndex != -1)
            _connectedPlayerWatchlists[note.Player].RemoveAt(deletedIndex);

        if (_connectedPlayerWatchlists[note.Player].Count == 0)
            _connectedPlayerWatchlists.Remove(note.Player);

        if (_playerManager.TryGetSessionById(note.Player, out var session))
            _admin.UpdatePlayerList(session);
    }
    // DeltaV - track watchlist changes END

    private void AddVerbs(GetVerbsEvent<Verb> ev)
    {
        if (EntityManager.GetComponentOrNull<ActorComponent>(ev.User) is not {PlayerSession: var user} ||
            EntityManager.GetComponentOrNull<ActorComponent>(ev.Target) is not {PlayerSession: var target})
        {
            return;
        }

        if (!_notes.CanView(user))
        {
            return;
        }

        var verb = new Verb
        {
            Text = Loc.GetString("admin-notes-verb-text"),
            Category = VerbCategory.Admin,
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/examine.svg.192dpi.png")),
            Act = () => _console.RemoteExecuteCommand(user, $"{OpenAdminNotesCommand.CommandName} \"{target.UserId}\""),
            Impact = LogImpact.Low
        };

        ev.Verbs.Add(verb);
    }

    private async void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        // DeltaV - clear watchlist cache on disconnect START
        if (e.NewStatus == SessionStatus.Disconnected)
        {
            _connectedPlayerWatchlists.Remove(e.Session.UserId);
            return;
        }
        // DeltaV - clear watchlist cache on disconnect END

        if (e.NewStatus != SessionStatus.InGame)
            return;

        var messages = await _notes.GetNewMessages(e.Session.UserId);
        var watchlists = await _notes.GetActiveWatchlists(e.Session.UserId);

        // DeltaV - write to cache START
        if (watchlists.Count != 0)
            _connectedPlayerWatchlists[e.Session.UserId] = watchlists.Select(r => r.ToShared()).ToList();
        // DeltaV - write to cache END

        if (!_playerManager.TryGetPlayerData(e.Session.UserId, out var playerData))
        {
            Log.Error($"Could not get player data for ID {e.Session.UserId}");
        }

        var username = playerData?.UserName ?? e.Session.UserId.ToString();
        foreach (var watchlist in watchlists)
        {
            _chat.SendAdminAlert(Loc.GetString("admin-notes-watchlist", ("player", username), ("message", watchlist.Message)));
        }

        var messagesToShow = messages.OrderBy(x => x.CreatedAt).Where(x => !x.Dismissed).ToArray();
        if (messagesToShow.Length == 0)
            return;

        var ui = new AdminMessageEui(messagesToShow);
        _euis.OpenEui(ui, e.Session);
    }
}
