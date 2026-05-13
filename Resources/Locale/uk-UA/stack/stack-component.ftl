### UI

# Shown when a stack is examined in details range
comp-stack-examine-detail-count = {$count ->
    [one] There is [color={$markupCountColor}]{$count}[/color] thing
    *[other] There are [color={$markupCountColor}]{$count}[/color] things
} in the stack.

# Stack status control
comp-stack-status = Кількість: [color=white]{$count}[/color]

### Interaction Messages

# Shown when attempting to add to a stack that is full
comp-stack-already-full = Стек уже заповнений.

# Shown when a stack becomes full
comp-stack-becomes-full = Крила (Атлас, Класика)

# Text related to splitting a stack
comp-stack-split = Ви розділили стек.
comp-stack-split-halve = Розділіть навпіл
comp-stack-split-too-small = Стос замалий, щоб розділити його.