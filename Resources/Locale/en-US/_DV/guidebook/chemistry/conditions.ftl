reagent-effect-condition-guidebook-type-damage =
    { $max ->
        [2147483648] it has at least {NATURALFIXED($min, 2)} {$type} damage
        *[other] { $min ->
                    [0] it has at most {NATURALFIXED($max, 2)} {$type} damage
                    *[other] it has between {NATURALFIXED($min, 2)} and {NATURALFIXED($max, 2)} {$type} damage
                 }
    }

reagent-effect-condition-guidebook-group-damage =
    { $max ->
        [2147483648] it has at least {NATURALFIXED($min, 2)} total {$group} damage
        *[other] { $min ->
                    [0] it has at most {NATURALFIXED($max, 2)} total {$group} damage
                    *[other] it has between {NATURALFIXED($min, 2)} and {NATURALFIXED($max, 2)} total {$group} damage
                 }
    }
