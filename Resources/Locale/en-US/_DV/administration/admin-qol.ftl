# Commands
cmd-lslaws-desc = Lists laws of all lawbound entities or a specific player if specified
cmd-lslaws-help = lslaws [username]
cmd-lslaws-error-bad-player = Unable to find lawbound entity attached to that user.

cmd-lswatchlisted-desc = Prints an overview of all connected players with watchlists
cmd-lswatchlisted-help = lswatchlisted

cmd-getping-desc = Prints the specified player's current ping
cmd-getping-help = getping <username>
cmd-getping-err = Unable to find specified player

cmd-freeze-desc = Freezes and mutes the specified player
cmd-freeze-help = freeze <username> [username 2] [username 3] ...
cmd-freeze-success = Froze and muted {$username}.
cmd-freeze-err-already-frozen = {$username} is already frozen.

cmd-unfreeze-desc = Unfreezes and unmutes the specified player
cmd-unfreeze-help = unfreeze <username> [username 2] [username 3] ...
cmd-unfreeze-success = Unfroze and unmuted {$username}.
cmd-unfreeze-err-not-frozen = {$username} isn't frozen.

freeze-cmds-err-not-found = Unable to find player {$username}.

# UI
ui-options-admin-player-tab-mark-ghosted = Mark ghosted players
ui-options-admin-player-tab-mark-ghosted-tooltip = Ghosts will have a "(G)" added to their character names (e.g. "(G) Glip-Glub")

ui-options-admin-player-tab-mark-watchlisted = Mark watchlisted players
ui-options-admin-player-tab-mark-watchlisted-tooltip = Watchlisted players will have a "(WL)" added to their character names (e.g. "(WL) Confusion Bot 2007")
