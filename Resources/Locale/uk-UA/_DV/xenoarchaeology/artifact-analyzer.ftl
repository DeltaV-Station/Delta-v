analysis-console-extract-value = [font="Monospace" size=11][color=orange]Вузол:{$id}Дослідження:+{$value}[/color][/font]
# DeltaV - modified analysis-console-glimmer-value
analysis-console-glimmer-value = [font="Monospace" size=11][color=orange]Вузол:{$id}Блиск:+{$value}[/color][/font]
analysis-console-extract-none = [font="Monospace" size=11][color=orange]Жодного розблокованого вузла не залишилося точок для вилучення [/color][/font]
# DeltaV - modified analysis-console-total-research-value
analysis-console-extract-sum = [font="Monospace" size=11][color=orange]Усього досліджень:{$value}[/color][/font]
# DeltaV - modified analysis-console-total-glimmer-value
analysis-console-glimmer-sum = [font="Monospace" size=11][color=orange]Загальний блиск:{$value}[/color][/font]
# DeltaV - modified analysis-console-multiplier-value
analysis-console-glimmer-mult = [font="Monospace" size=11][color=orange]Поточний множник:{$value}[/color][/font]

analysis-console-info-effect-value = [font="Monospace" size=11][color=gray]{ $state ->
    [vagueandspecific] {$vagueInfo} ({$specificInfo})
    [vagueonly] {$vagueInfo} (unable to detect details)
    [simple] {$specificInfo}
    [hidden] Unable to detect (unlock to discover)
    *[noinfo] Unlock nodes to gain info
}[/color][/font]