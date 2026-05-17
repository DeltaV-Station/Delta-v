shared-solution-container-component-on-examine-main-text = Містить {INDEFINITE($desc)} [color={$color}]{$desc}[/color] { $chemCount ->
    [1] хімічну речовину.
   *[other] суміш хімічних речовин.
    }

examinable-solution-has-recognizable-chemicals = Ти впізнаєш {$recognizedString} у розчині.

examinable-solution-recognized = [color={$color}]{$chemical}[/color]

examinable-solution-on-examine-volume = Вміст розчину { $fillLevel ->
    [exact] становить [color=white]{$current}/{$max}u[/color].
   *[other] [bold]{ -solution-vague-fill-level(fillLevel: $fillLevel) }[/bold].
}

examinable-solution-on-examine-volume-no-max = Вміст розчину { $fillLevel ->
    [exact] становить [color=white]{$current}u[/color].
   *[other] [bold]{ -solution-vague-fill-level(fillLevel: $fillLevel) }[/bold].
}

examinable-solution-on-examine-volume-puddle = Калюжа { $fillLevel ->
    [exact] [color=white]{$current}u[/color].
    [full] величезна та переливається!
    [mostlyfull] величезна та переливається!
    [halffull] глибока та тече.
    [halfempty] дуже глибока.
   *[mostlyempty] збирається докупи.
    [empty] утворює кілька малих калюж.
}

-solution-vague-fill-level = { $fillLevel ->
        [full] [color=white]Повне[/color]
        [mostlyfull] [color=#DFDFDF]Майже повне[/color]
        [halffull] [color=#C8C8C8]Наполовину повне[/color]
        [halfempty] [color=#C8C8C8]Наполовину порожнє[/color]
        [mostlyempty] [color=#A4A4A4]Майже порожнє[/color]
       *[empty] [color=gray]Порожнє[/color]
    }
