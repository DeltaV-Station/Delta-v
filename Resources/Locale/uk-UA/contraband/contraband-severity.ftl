contraband-examine-text-Minor =
    { $type ->
        *[item] [color=yellow]This item is considered minor contraband.[/color]
        [reagent] [color=yellow]This reagent is considered minor contraband.[/color]
    }

contraband-examine-text-Restricted =
    { $type ->
        *[item] [color=yellow]This item is departmentally restricted.[/color]
        [reagent] [color=yellow]This reagent is departmentally restricted.[/color]
    }

contraband-examine-text-Restricted-department =
    { $type ->
        *[item] [color=yellow]This item is restricted to {$departments}, and may be considered contraband.[/color]
        [reagent] [color=yellow]This reagent is restricted to {$departments}, and may be considered contraband.[/color]
    }

contraband-examine-text-Major =
    { $type ->
        *[item] [color=red]This item is considered major contraband.[/color]
        [reagent] [color=red]This reagent is considered major contraband.[/color]
    }

contraband-examine-text-GrandTheft =
    { $type ->
        *[item] [color=red]This item is a highly valuable target for Syndicate agents![/color]
        [reagent] [color=red]This reagent is a highly valuable target for Syndicate agents![/color]
    }

contraband-examine-text-Highly-Illegal =
    { $type ->
        *[item] [color=crimson]This item is highly illegal contraband![/color]
        [reagent] [color=crimson]This reagent is highly illegal contraband![/color]
    }

contraband-examine-text-Syndicate =
    { $type ->
        *[item] [color=crimson]This item is highly illegal Syndicate contraband![/color]
        [reagent] [color=crimson]This reagent is highly illegal Syndicate contraband![/color]
    }

contraband-examine-text-Magical =
    { $type ->
        *[item] [color=#b337b3]This item is highly illegal magical contraband![/color]
        [reagent] [color=#b337b3]This reagent is highly illegal magical contraband![/color]
    }

contraband-examine-text-avoid-carrying-around = [color=red][italic]Ви, ймовірно, захочете не носити це на видноті без поважної причини.[/italic][/color]
contraband-examine-text-in-the-clear = [color=green][italic]Ви повинні бути на відкритому місці, щоб бачити це з собою.[/italic][/color]

contraband-examinable-verb-text = Законність
contraband-examinable-verb-message = Перевірити законність даного товару.

contraband-department-plural = Тепер RCD налаштовано на створення {MAKEPLURAL($name)}.
contraband-job-plural = {MAKEPLURAL($job)}