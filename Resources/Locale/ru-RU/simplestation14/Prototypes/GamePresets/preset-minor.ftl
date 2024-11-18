## неопасный антагонист

# Shown at the end of a round of minor antags
minor-round-end-result = {$minorCount ->
    [one] Был один неопасный антагонист.
    *[other] Было {$minorCount} неопасных антагонистов.
}

# Показывается в конце раунда неопасных антагонистов.
minor-user-was-a-minor = [color=gray]{$user}[/color] был неопасный антагонист.
minor-user-was-a-minor-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) был неопасный антагонист.
minor-was-a-minor-named = [color=White]{$name}[/color] был неопасный антагонист.

minor-user-was-a-minor-with-objectives = [color=gray]{$user}[/color] был неопасным антагонистом, у которого была следующая цель:
minor-user-was-a-minor-with-objectives-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) был неопасным антагонистом, у которого была следующая цель:
minor-was-a-minor-with-objectives-named = [color=White]{$name}[/color] был неопасным антагонистом, у которого была следующая цель:

preset-minor-objective-issuer-freewill = [color=#87cefa]Искажение разума[/color]

minor-objective-condition-success = {$condition} | [color={$markupColor}]Успех![/color]
minor-objective-condition-fail = {$condition} | [color={$markupColor}]Провал![/color] ({$progress}%)

preset-minor-title = неопасный антагонист
preset-minor-description = Этот игровой режим не должен использоваться...
preset-minor-not-enough-ready-players = Недостаточно игроков подготовились к игре! Было подготовлено {$readyPlayersCount} игроков из {$minimumPlayers} необходимых.
preset-minor-no-one-ready = Нет готовых игроков! Невозможно начать режим неопасных антагонистов.

## minorRole

# minorRole
minor-role-greeting =
    Вы являетесь неопасным антагонистом с малыми задачами.