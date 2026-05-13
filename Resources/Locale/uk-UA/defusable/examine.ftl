defusable-examine-defused = {CAPITALIZE(THE($name))} [color=lime]знешкоджено[/color].
defusable-examine-live = {CAPITALIZE(THE($name))} [color=red]цокає[/color]і залишилося [color=red]{$time}[/color]секунд.
defusable-examine-live-display-off = {CAPITALIZE(THE($name))} [color=red]цокає[/color], а таймер, здається, вимкнено.
defusable-examine-inactive = {CAPITALIZE(THE($name))} [color=lime]неактивний[/color], але все ще може бути під озброєнням.
defusable-examine-bolts = Болти {$вниз ->
[true] [color=red]down[/color]
*[false] [color=green]up[/color]
}.