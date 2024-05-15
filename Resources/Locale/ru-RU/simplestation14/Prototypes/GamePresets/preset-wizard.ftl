## wizard
# Shown at the end of a round of wizard
wizard-round-end-result = {$wizardCount ->
    [one] Был один волшебник.
    *[other] Было {$wizardCount} волшебников.
}

# Shown at the end of a round of wizard
wizard-user-was-a-wizard = [color=gray]{$user}[/color] был волшебником.
wizard-user-was-a-wizard-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) был волшебником.
wizard-was-a-wizard-named = [color=White]{$name}[/color] был волшебником.

wizard-user-was-a-wizard-with-objectives = [color=gray]{$user}[/color] был волшебником, у которого были следующие цели:
wizard-user-was-a-wizard-with-objectives-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) был волшебником, у которого были следующие цели:
wizard-was-a-wizard-with-objectives-named = [color=White]{$name}[/color] был волшебником, у которого были следующие цели:

preset-wizard-objective-issuer-wizfeds = [color=#87cefa]Федерация волшебников[/color]

wizard-objective-condition-success = {$condition} | [color={$markupColor}]Успех![/color]
wizard-objective-condition-fail = {$condition} | [color={$markupColor}]Провал![/color] ({$progress}%)

preset-wizard-title = Волшебник
preset-wizard-description = Волшебники прячутся среди экипажа станции, найдите и разберитесь с ними, пока они не стали слишком могущественными.
preset-wizard-not-enough-ready-players = Недостаточно игроков готово к игре! Было подготовлено {$readyPlayersCount} игроков из {$minimumPlayers} необходимых.
preset-wizard-no-one-ready = Нет готовых игроков! Невозможно запустить мастера.

## wizardRole

# wizardRole
wizard-role-greeting =
    Вы - оперативник Федерации волшебников, работающий под прикрытием.
    Ваши задачи и кодовые слова перечислены в меню персонажа.
    Воспользуйтесь каналом связи, загруженным в ваш КПК, чтобы купить инструменты, необходимые для выполнения задания.
    Больше валюты можно получить, выполняя задания Оракула.
    Смерть Нанотрасену!
wizard-role-codewords =
    Кодовыми словами являются:
    {$codewords}.
    Кодовые слова можно использовать в обычном разговоре, чтобы незаметно для других оперативников идентифицировать себя.
    Прислушивайтесь к ним и держите их в секрете.