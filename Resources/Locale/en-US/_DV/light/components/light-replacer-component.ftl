
### Interaction Messages

# Shown when player tries to replace light, but there is no lights left
comp-light-replacer-missing-light-dv = No {$light-name}s left in {THE($light-replacer)}.

# Shown when a player attempts to replace a light with the same color & type as the active light.
comp-light-replacer-same-light = This fixture already holds {INDEFINITE($light-name)} {$light-name}!

# Radial Menu messages
comp-light-replacer-eject-specified-lights = Eject all {$light-name}s.
comp-light-replacer-select-lights = Select {$light-name}s.
comp-light-replacer-open-empty = {CAPITALIZE(THE($light-replacer))} is completely empty!

# Label
comp-light-replacer-label = Tube: {$tube}
                            Bulb: {$bulb}

### Examine

comp-light-replacer-light-listing-dv = {$amount ->
    [one] [color=yellow]{$amount}[/color] [color=gray]{$light-name}[/color]
    *[other] [color=yellow]{$amount}[/color] [color=gray]{$light-name}s[/color]
}
