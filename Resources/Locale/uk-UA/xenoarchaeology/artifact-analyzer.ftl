analysis-console-menu-title = аналiтична консоль
analysis-console-server-list-button = Список серверів
analysis-console-extract-button = Вилучити

analysis-console-info-no-scanner = Аналізатор не підключено! Будь ласка, підключіть його за допомогою мультітула.
analysis-console-info-no-artifact = Артефакт відсутній! Помістіть один на платформу і проскануйте.
analysis-console-info-ready = Системи працюють. Готовий до сканування.

analysis-console-no-node = Виберіть вузол для перегляду
analysis-console-info-id = [font="Monospace" size=11]Ідентифікатор:[/font]
analysis-console-info-id-value = [font="Monospace" size=11][color=yellow]{$id}[/color][/font]
analysis-console-info-class = [font="Monospace" size=11]Клас:[/font]
analysis-console-info-class-value = [font="Monospace" size=11]{$class}[/font]
analysis-console-info-locked = [font="Monospace" size=11]Статус:[/font]
analysis-console-info-locked-value = [font="Monospace" size=11][color={ $state ->
    [0] red]Locked
    [1] lime]Unlocked
    *[2] plum]Active
}[/color][/font]
analysis-console-info-durability = [font="Monospace" size=11]Стійкість:[/font]
analysis-console-info-durability-value = [font="Monospace" size=11][color={$color}]{$current}/{$max}[/color][/font]
analysis-console-info-effect = РЕАКЦІЯ: {$effect}
# DeltaV - moved to _DV file
#analysis-console-info-effect-value = [font="Monospace" size=11][color=gray]{ $state ->
#    [true] {$info}
#    *[false] Unlock nodes to gain info
#}[/color][/font]
analysis-console-info-trigger = СТИМУЛ: {$trigger}
analysis-console-info-triggered-value = [font="Monospace" size=11][color=gray]{$triggers}[/color][/font]
analysis-console-info-scanner = Сканування...
analysis-console-info-scanner-paused = Призупинено.
analysis-console-progress-text = {$seconds ->
    [one] T-{$seconds} second
    *[other] T-{$seconds} seconds
}

#analysis-console-extract-value = [font="Monospace" size=11][color=orange]Node:{$id} Research:+{$value}[/color][/font]
# DeltaV - modified analysis-console-glimmer-value - moved to DV file
#analysis-console-glimmer-value = [font="Monospace" size=11][color=orange]Node:{$id} Glimmer:+{$value}[/color][/font]
#analysis-console-extract-none = [font="Monospace" size=11][color=orange]No unlocked nodes have any points left to extract [/color][/font]
# DeltaV - modified analysis-console-total-research-value - moved to DV file
#analysis-console-extract-sum = [font="Monospace" size=11][color=orange]Total Research:{$value}[/color][/font]
# DeltaV - modified analysis-console-total-glimmer-value - moved to DV file
#analysis-console-glimmer-sum = [font="Monospace" size=11][color=orange]Total Glimmer:{$value}[/color][/font]
# DeltaV - modified analysis-console-multiplier-value - moved to DV file
#analysis-console-glimmer-mult = [font="Monospace" size=11][color=orange]Current Multiplier:{$value}[/color][/font]

analyzer-artifact-extract-popup = Енергія мерехтить на поверхні артефакту!