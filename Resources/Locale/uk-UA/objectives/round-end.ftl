objectives-round-end-result = {$count ->
    [one] There was one {$agent}.
    *[other] There were {$count} {MAKEPLURAL($agent)}.
}

objectives-round-end-result-in-custody = {$custody} з {$count} {MAKEPLURAL($agent)} було затримано.

objectives-player-user-named = [color=White]{$name}[/color]([color=gray]{$user}[/color])
objectives-player-named = [color=White]{$name}[/color]

objectives-no-objectives = [bold][color=red]{$custody}[/color]{$title} були {$agent}.
objectives-with-objectives = [bold][color=red]{$custody}[/color]{$title} були {$agent} і мали наступні завдання:

objectives-objective-success = {$objective} | [color={$markupColor}]Успіх![/color]
objectives-objective-partial-success = {$objective}| [color=yellow]Частковий успіх![/color]({TOSTRING($progress, "P0")})
objectives-objective-partial-failure = {$objective}| [color=orange]Часткова помилка![/color]({TOSTRING($progress, "P0")})
objectives-objective-fail = {$objective} | [color={$markupColor}]Невдача![/color] ({$progress}%)

objectives-in-custody = | ЗАТРИМАНО |