entity-condition-guidebook-total-damage =
    { $max ->
        [2147483648] має принаймні {NATURALFIXED($min, 2)} загальних пошкоджень
        *[other] { $min ->
                    [0] має максимум {NATURALFIXED($max, 2)} загальних пошкоджень
                    *[other] має між {NATURALFIXED($min, 2)} та {NATURALFIXED($max, 2)} загальних пошкоджень
                 }
    }

entity-condition-guidebook-type-damage =
    { $max ->
        [2147483648] має принаймні {NATURALFIXED($min, 2)} пошкоджень типу {$type}
        *[other] { $min ->
                    [0] має максимум {NATURALFIXED($max, 2)} пошкоджень типу {$type}
                    *[other] має між {NATURALFIXED($min, 2)} та {NATURALFIXED($max, 2)} пошкоджень типу {$type}
                 }
    }

entity-condition-guidebook-group-damage =
    { $max ->
        [2147483648] має принаймні {NATURALFIXED($min, 2)} пошкоджень типу {$type}.
        *[other] { $min ->
                    [0] має максимум {NATURALFIXED($max, 2)} пошкоджень типу {$type}.
                    *[other] має між {NATURALFIXED($min, 2)} та {NATURALFIXED($max, 2)} пошкоджень типу {$type}
                 }
    }

entity-condition-guidebook-total-hunger =
    { $max ->
        [2147483648] ціль має принаймні {NATURALFIXED($min, 2)} загального голоду
        *[other] { $min ->
                    [0] ціль має максимум {NATURALFIXED($max, 2)} загального голоду
                    *[other] ціль має між {NATURALFIXED($min, 2)} та {NATURALFIXED($max, 2)} загального голоду
                 }
    }

entity-condition-guidebook-reagent-threshold =
    { $max ->
        [2147483648] є принаймні {NATURALFIXED($min, 2)}u {$reagent}
        *[other] { $min ->
                    [0] є максимум {NATURALFIXED($max, 2)}u {$reagent}
                    *[other] є між {NATURALFIXED($min, 2)}u та {NATURALFIXED($max, 2)}u {$reagent}
                 }
    }

entity-condition-guidebook-mob-state-condition =
    моб знаходиться у стані { $state }

entity-condition-guidebook-job-condition =
    посада цілі { $job }

entity-condition-guidebook-solution-temperature =
    температура розчину { $max ->
            [2147483648] принаймні {NATURALFIXED($min, 2)}k
            *[other] { $min ->
                        [0] максимум {NATURALFIXED($max, 2)}k
                        *[other] між {NATURALFIXED($min, 2)}k та {NATURALFIXED($max, 2)}k
                     }
    }

entity-condition-guidebook-body-temperature =
    температура тіла { $max ->
            [2147483648] принаймні {NATURALFIXED($min, 2)}k
            *[other] { $min ->
                        [0] максимум {NATURALFIXED($max, 2)}k
                        *[other] між {NATURALFIXED($min, 2)}k та {NATURALFIXED($max, 2)}k
                     }
    }

entity-condition-guidebook-organ-type =
    метаболізуючий орган { $shouldhave ->
                                [true] є
                                *[false] не є
                           } {INDEFINITE($name)} {$name} органом

entity-condition-guidebook-has-tag =
    ціль { $invert ->
                 [true] не має
                 *[false] має
                } тег {$tag}

entity-condition-guidebook-this-reagent = цей реагент

entity-condition-guidebook-breathing =
    метаболізатор { $isBreathing ->
                [true] дихає нормально
                *[false] задихається
               }

entity-condition-guidebook-internals =
    метаболізатор { $usingInternals ->
                [true] використовує внутрішні запаси
                *[false] дихає атмосферним повітрям
               }
