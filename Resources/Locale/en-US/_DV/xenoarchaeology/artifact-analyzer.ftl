analysis-console-extract-value = [font="Monospace" size=11][color=orange]Node:{$id} Research:+{$value}[/color][/font]
# DeltaV - modified analysis-console-glimmer-value
analysis-console-glimmer-value = [font="Monospace" size=11][color=orange]Node:{$id} Glimmer:+{$value}[/color][/font]
analysis-console-extract-none = [font="Monospace" size=11][color=orange]No unlocked nodes have any points left to extract [/color][/font]
# DeltaV - modified analysis-console-total-research-value
analysis-console-extract-sum = [font="Monospace" size=11][color=orange]Total Research:{$value}[/color][/font]
# DeltaV - modified analysis-console-total-glimmer-value
analysis-console-glimmer-sum = [font="Monospace" size=11][color=orange]Total Glimmer:{$value}[/color][/font]
# DeltaV - modified analysis-console-multiplier-value
analysis-console-glimmer-mult = [font="Monospace" size=11][color=orange]Current Multiplier:{$value}[/color][/font]

analysis-console-info-effect-value = [font="Monospace" size=11][color=gray]{ $state ->
    [vagueandspecific] {$vagueInfo} ({$specificInfo})
    [vagueonly] {$vagueInfo} (unable to detect details)
    [simple] {$specificInfo}
    [hidden] Unable to detect (unlock to discover)
    *[noinfo] Unlock nodes to gain info
}[/color][/font]
