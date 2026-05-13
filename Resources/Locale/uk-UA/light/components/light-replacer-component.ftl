
### Interaction Messages

# Shown when player tries to replace light, but there is no lights left
comp-light-replacer-missing-light = У {THE($light-replacer)} не залишилося світла.

# Shown when player inserts light bulb inside light replacer
comp-light-replacer-insert-light = Ви вставляєте {$bulb}у {THE($light-replacer)}.

# Shown when player tries to insert in light replacer brolen light bulb
comp-light-replacer-insert-broken-light = Не можна вставляти розбиті ліхтарі!

# Shown when player refill light from light box
comp-light-replacer-refill-from-storage = Ви заповнюєте {THE($light-replacer)}.

### Examine 

comp-light-replacer-no-lights = Він порожній.
comp-light-replacer-has-lights = Він містить наступне:
comp-light-replacer-light-listing = {$сума ->
    [one] [color=yellow]{$amount}[/color] [color=gray]{$name}[/color]
    *[other] [color=yellow]{$amount}[/color] [color=gray]{$name}s[/color]
}