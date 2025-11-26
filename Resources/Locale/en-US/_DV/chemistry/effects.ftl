reagent-effect-guidebook-chem-remove-psionic =
    { $chance ->
        [1] Removes
        *[other] {$chance} to remove
    } all psionic powers

reagent-effect-guidebook-chem-roll-psionic =
    Has a chance to grant another, different psionic power
    { $multiplier ->
        [1] power
        *[other] power with a chance multiplier of {$multiplier}
    }
