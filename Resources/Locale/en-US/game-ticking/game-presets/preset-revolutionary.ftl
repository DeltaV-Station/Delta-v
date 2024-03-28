## Rev Head

roles-antag-rev-head-name = Head Revolutionary
roles-antag-rev-head-objective = Your objective is to take over the station by converting people to your cause and mooring all Command staff on station.

head-rev-role-greeting =
    You are a Head Revolutionary.
    You are tasked with assuming control of the station and mooring all Command staff.
    The Syndicate has sponsored you with a flash that converts the crew to your side.
    Beware, this won't work on Security, Command, or those wearing sunglasses.
    Viva la revolución!

head-rev-briefing =
    Use flashes to convert people to your cause.
    Take over the station and moor all members of Command.

head-rev-break-mindshield = The Mindshield was destroyed!

## Rev

roles-antag-rev-name = Revolutionary
roles-antag-rev-objective = Your objective is to support the Head Revolutionaries in their endeavor to seize control of the station.

rev-break-control = {$name} has remembered their true allegiance!

rev-role-greeting =
    You are a Revolutionary.
    Your mind has been altered so that you will support and protect the Head Revolutionaries.
    You are the same person you always were; you just see things differently, now.
    You will remember none of this when it is over.
    Viva la revolución!

rev-briefing = Assist the Head Revolutionaries in seizing control of the station.

## General

rev-title = Revolutionaries
rev-description = Revolutionaries are among us.

rev-not-enough-ready-players = Not enough players readied up for the game. There were {$readyPlayersCount} players readied up out of {$minimumPlayers} needed. Can't start a Revolution.
rev-no-one-ready = No players readied up! Can't start a Revolution.
rev-no-heads = There were no Head Revolutionaries to be selected. Can't start a Revolution.

rev-all-heads-dead = All the heads are dead!

rev-won = The Head Revs survived and moored all of Command.

rev-lost = Command survived and neutralized all of the Head Revs.

rev-stalemate = All of the Head Revs and Command were defeated. It's a draw.

rev-reverse-stalemate = Both Command and Head Revs survived.

rev-headrev-count = {$initialCount ->
    [one] There was one Head Revolutionary:
    *[other] There were {$initialCount} Head Revolutionaries:
}

rev-headrev-name-user = [color=#5e9cff]{$name}[/color] ([color=gray]{$username}[/color]) converted {$count} {$count ->
    [one] person
    *[other] people
}

rev-headrev-name = [color=#5e9cff]{$name}[/color] converted {$count} {$count ->
    [one] person
    *[other] people
}

## Deconverted window

rev-deconverted-title = Deconverted!
rev-deconverted-text =
    As the last headrev has been neutralized, the revolution is over.

    You are no longer a revolutionary, and only remember a bright flash in your eyes.
rev-deconverted-confirm = Confirm
