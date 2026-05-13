guidebook-reagent-effect-description =
    {$quantity ->
        [0] {""}
        *[other] If there is at least {$quantity}u {$reagent},{" "}
    }{$chance ->
        [1] { $effect }
        *[other] Has a { NATURALPERCENT($chance, 2) } chance to { $effect }
    }{ $conditionCount ->
        [0] .
        *[other] {" "}when { $conditions }.
    }

guidebook-reagent-name = [bold][color={$color}]{CAPITALIZE($name)}[/color][/bold]
guidebook-reagent-recipes-header = рецепт
guidebook-reagent-recipes-reagent-display = [bold]{$reagent}[/bold]\[{$ratio}\]
guidebook-reagent-sources-header = Джерела
guidebook-reagent-sources-ent-wrapper = [bold]{$name}[/bold]\[1\]
guidebook-reagent-sources-gas-wrapper = [bold]{$name}(газ)[/bold]\[1\]
guidebook-reagent-effects-header = Ефекти
guidebook-reagent-effects-metabolism-group-rate = [bold]{$group}[/bold][color=gray]({$rate}одиниць за секунду)[/color]
guidebook-reagent-plant-metabolisms-header = Метаболізм рослин
guidebook-reagent-plant-metabolisms-rate = [bold]Метаболізм рослин[/bold][color=gray](1 одиниця кожні 3 секунди як основа)[/color]
guidebook-reagent-physical-description = [italic]Здається, {$description}.[/italic]
guidebook-reagent-recipes-mix-info = {$minTemp ->
    [0] {$hasMax ->
            [true] {CAPITALIZE($verb)} below {NATURALFIXED($maxTemp, 2)}K
            *[false] {CAPITALIZE($verb)}
        }
    *[other] {CAPITALIZE($verb)} {$hasMax ->
            [true] between {NATURALFIXED($minTemp, 2)}K and {NATURALFIXED($maxTemp, 2)}K
            *[false] above {NATURALFIXED($minTemp, 2)}K
        }
}