## UI

injector-volume-transfer-label = Обсяг: [color=white]{$currentVolume}/{$totalVolume}u[/color]
    Mode: [color=white]{$modeString}[/color] ([color=white]{$transferVolume}u[/color])
injector-volume-label = Кількість: [color=white]{$currentVolume}/{$totalVolume}[/color]
    Mode: [color=white]{$modeString}[/color]
injector-toggle-verb-text = Перемкнути режим інжектора

## Entity

injector-component-inject-mode-name = Вводити
injector-component-draw-mode-name = малювати
injector-component-dynamic-mode-name = Динамічний
injector-component-mode-changed-text = Зараз {$mode}
injector-component-transfer-success-message = Ви перемістили {$amount}u в {$target}.
injector-component-transfer-success-message-self = Ви переносите {$amount}u в себе.
injector-component-inject-success-message = Ви ввели {$amount}u в {$target}!
injector-component-inject-success-message-self = Ви вводите {$amount}u в себе!
injector-component-draw-success-message = Ви набрали {$amount}u з {$target}.
injector-component-draw-success-message-self = Ви черпаєте {$amount}u з себе.

## Fail Messages

injector-component-target-already-full-message = {$target} вже повний!
injector-component-target-already-full-message-self = Ви вже ситі!
injector-component-target-is-empty-message = {$target} пустий!
injector-component-target-is-empty-message-self = Ви порожні!
injector-component-cannot-toggle-draw-message = Занадто повний, щоб малювати!
injector-component-cannot-toggle-inject-message = Нічого колоти!
injector-component-cannot-toggle-dynamic-message = Неможливо перемкнути динаміку!
injector-component-empty-message = {CAPITALIZE(THE($injector))} порожній!
injector-component-blocked-user = Захисне спорядження заблокувало вашу ін'єкцію!
injector-component-blocked-other = {CAPITALIZE(THE(POSS-ADJ($target)))} броня заблокувала ін’єкцію {THE($user)}!
injector-component-cannot-transfer-message = Ви не можете перемістити речовину в {$target}!
injector-component-cannot-transfer-message-self = Ви не можете перейти в себе!
injector-component-cannot-inject-message = Ви не можете ввести речовину в {$target}!
injector-component-cannot-inject-message-self = Ви не вмієте робити собі ін'єкції!
injector-component-cannot-draw-message = Ви не можете набрати речовину з {$target}!
injector-component-cannot-draw-message-self = Ви не вмієте черпати з себе!
injector-component-ignore-mobs = Цей інжектор може взаємодіяти тільки з контейнерами!

## mob-inject doafter messages
injector-component-needle-injecting-user = Ви починаєте вводити голку.
injector-component-needle-injecting-target = {CAPITALIZE(THE($user))} намагається вколоти вас голкою!
injector-component-needle-drawing-user = Ви починаєте малювати голку.
injector-component-needle-drawing-target = {CAPITALIZE(THE($user))} намагається використати голку, щоб зняти з вас!
injector-component-spray-injecting-user = Ви починаєте готувати розпилювач.
injector-component-spray-injecting-target = {CAPITALIZE(THE($user))} намагається поставити на вас розпилювач!

## Target Popup Success messages
injector-component-feel-prick-message = Ви відчуваєте маленький укол!