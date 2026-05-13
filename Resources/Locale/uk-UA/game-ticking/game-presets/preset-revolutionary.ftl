## Rev Head

roles-antag-rev-head-name = Головний Революціонер
roles-antag-rev-head-objective = Ваша мета - захопити станцію, вербуючи людей на свій бік і вбивши всіх членів командування станції.

head-rev-role-greeting =
    You are a head revolutionary. You are tasked with removing all of Command from power through death, restraint, or conversion.
    The Syndicate has sponsored you with a flash that converts others to your cause. Beware, this won't work on those with eye protection or mindshield implants. Remember that Command and Security are implanted with mindshields as part of the hiring process.
    Viva la revolución!

head-rev-briefing =
    Use flashes to convert people to your cause.
    Kill, restrain, or convert all members of Command to take over the station.

head-rev-break-mindshield = Імплант лояльності знищено!

## Rev

roles-antag-rev-name = Революціонер
roles-antag-rev-objective = Ваша мета - забезпечити безпеку та виконувати накази Головних Революціонерів, а також вбити всіх членів командування станції.

rev-break-control = {$name} згадав свою істинну приналежність!

rev-role-greeting =
    You are a revolutionary. You are tasked with protecting the head revolutionaries and helping them take over the station.
    The revolution must work together to kill, restrain, or convert all members of Command.
    Viva la revolución!

rev-briefing = Допоможіть Головним Революціонерам вбити всіх членів командування, щоб захопити станцію.

## General

rev-title = Революціонери
rev-description = Революціонери на станції.

rev-not-enough-ready-players = Недостатньо готових гравців. Було готово {$readyPlayersCount} з {$minimumPlayers} необхідних. Неможливо розпочати Революцію.
rev-no-one-ready = Жоден з гравців не приготувався! Неможливо розпочати Революцію.
rev-no-heads = Не було обрано жодного Головного Революціонера. Неможливо розпочати революцію.

rev-won = Головні Революціонери вижили та вбили всіх членів Командування.

rev-lost = Командування вижило і вбило всіх Головних Революціонерів.

rev-stalemate = Всі Головні Революціонери та Командування станції загинули. І як так сталося... Назвемо це нічиєю.

rev-reverse-stalemate = І Командування станції та Головні Революціонери вижили. Виграла... дружба?

rev-headrev-count = {$initialCount ->
    [one] There was one head revolutionary:
    *[other] There were {$initialCount} head revolutionaries:
}

rev-headrev-name-user = [color=#5e9cff]{$name}[/color] ([color=gray]{$username}[/color]) завербував {$count} {$count ->
    [one] person
    *[other] people
}

rev-headrev-name = [color=#5e9cff]{$name}[/color] завербував {$count} {$count ->
    [one] person
    *[other] people
}

## Deconverted window

rev-deconverted-title = Деконвертовано!
rev-deconverted-text =
    As the last head revolutionary has died, the revolution is over.

    You are no longer a revolutionary, so be nice.
rev-deconverted-confirm = Підтвердити