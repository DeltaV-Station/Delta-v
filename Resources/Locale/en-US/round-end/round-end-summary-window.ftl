round-end-summary-window-title = Round End Summary
round-end-summary-window-round-end-summary-tab-title = Round Information
round-end-summary-window-player-manifest-tab-title = Player Manifest
round-end-summary-window-round-id-label = Round [color=white]#{$roundId}[/color] has ended.
round-end-summary-window-gamemode-name-label = The game mode was [color=white]{$gamemode}[/color].
round-end-summary-window-duration-label = It lasted for [color=yellow]{$hours} hours, {$minutes} minutes, and {$seconds} seconds.
round-end-summary-window-player-info-if-observer-text = [color=gray]{$playerOOCName}[/color] was [color=lightblue]{$playerICName}[/color], an observer.
round-end-summary-window-player-info-if-not-observer-text = [color=gray]{$playerOOCName}[/color] was [color={$icNameColor}]{$playerICName}[/color] playing role of [color=orange]{$playerRole}[/color].

round-end-summary-window-commendations-tab-title = Crew Commendation
round-end-summary-window-commendations-header = [bold]Crew Commendation[/bold]
round-end-summary-window-commendations-subheader = Choose the player who roleplayed best this round.
round-end-summary-window-commendations-search-placeholder = Search by name...
round-end-summary-window-commendations-player-display = [bold]{$icName}[/bold] ({$oocName}) — {$role} [color=gold]+{$total} (+{$round} per round)[/color]
round-end-summary-window-commendations-already-voted = [color=yellow]You have already voted this round![/color]
round-end-summary-window-commendations-thanks = [color=green]You have commended this player! Thanks for voting![/color]
round-end-summary-window-commendations-confirm-title = Confirmation
round-end-summary-window-commendations-confirm-text = Are you sure you want to commend player {$player}? You can only give one vote per round.

# Server side messages
commendation-server-player-not-found = Player not found.
commendation-server-self-commend = You cannot commend yourself.
commendation-server-already-commended = You have already commended someone this round.
commendation-server-insufficient-round-time = The round duration was too short ({$min} min minimum) to vote.
commendation-server-insufficient-player-time = You haven't spent enough time in the round ({$min} min minimum) to vote.
commendation-server-receiver-notification = You have just been commended for good roleplay in round #{$roundId}! Thank you for your play!
