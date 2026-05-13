## UI
cargo-console-menu-title = Консоль подачі заявок на вантаж
cargo-console-menu-account-name-label = Назва рахунку:{" "}
cargo-console-menu-account-name-none-text = Немає
cargo-console-menu-account-name-format = [bold][color={$color}]{$name}[/color][/bold][font="Monospace"]\[{$code}\][/font]
cargo-console-menu-shuttle-name-label = Ім'я шатла{" "}
cargo-console-menu-shuttle-name-none-text = Немає
cargo-console-menu-points-label = Космобакси{" "}
cargo-console-menu-points-amount = ${$amount}
cargo-console-menu-shuttle-status-label = Статус шатлу{" "}
cargo-console-menu-shuttle-status-away-text = Геть
cargo-console-menu-order-capacity-label = Обсяг замовлення:{" "}
cargo-console-menu-call-shuttle-button = Активувати телепанель
cargo-console-menu-permissions-button = Дозволи
cargo-console-menu-categories-label = Категорії:{" "}
cargo-console-menu-search-bar-placeholder = Пошук
cargo-console-menu-requests-label = Заявки
cargo-console-menu-orders-label = Замовлення
cargo-console-menu-order-reason-description = Причини: {$reason}
cargo-console-menu-populate-categories-all-text = Усі
cargo-console-menu-populate-orders-cargo-order-row-product-name-text = {$productName} (x{$orderAmount}) по {$orderRequester}
cargo-console-menu-cargo-order-row-approve-button = Затвердити
cargo-console-menu-cargo-order-row-cancel-button = Закрити
cargo-console-menu-tab-title-orders = Замовлення
cargo-console-menu-tab-title-funds = Трансфери
cargo-console-menu-account-action-transfer-limit = [bold]Ліміт переказу:[/bold]${$limit}
cargo-console-menu-account-action-transfer-limit-unlimited-notifier = [color=gold](Необмежено)[/color]
cargo-console-menu-account-action-select = [bold]Дія облікового запису:[/bold]
cargo-console-menu-account-action-amount = [bold]Сума:[/bold]$
cargo-console-menu-account-action-button = Трансфер
cargo-console-menu-toggle-account-lock-button = Переключити ліміт передачі
cargo-console-menu-account-action-option-withdraw = Зняти готівку
cargo-console-menu-account-action-option-transfer = Переказ коштів на {$code}

# Orders
cargo-console-order-not-allowed = Доступ заборонено
cargo-console-station-not-found = Немає доступної станції
cargo-console-invalid-product = Невірний ідентифікатор товару
cargo-console-too-many = Занадто багато затверджених наказів
cargo-console-snip-snip = Замовлення урізано до мінімуму
cargo-console-insufficient-funds = Недостатність коштів (require {$cost})
cargo-console-unfulfilled = Невистачає місця для виконання
cargo-console-trade-station = Відправити до {$destination}
cargo-console-unlock-approved-order-broadcast = [bold]{$productName}x{$orderAmount}[/bold], що коштує [bold]{$cost}[/bold], було схвалено [bold]{$approver}[/bold]
cargo-console-fund-withdraw-broadcast = [bold]{$name}відкликав {$amount}Spesos з {$name1}\[{$code1}\]
cargo-console-fund-transfer-broadcast = [bold]{$name}передав {$amount}Spesos від {$name1}\[{$code1}\] до {$name2}\[{$code2}\][/bold]
cargo-console-fund-transfer-user-unknown = Невідомий

cargo-console-paper-reason-default = Жодного
cargo-console-paper-approver-default = себе
cargo-console-paper-print-name = Замовлення #{$orderNumber}
cargo-console-paper-print-text = Замовлення: #{$orderNumber}
    {"[bold]Item:[/bold]"} {$itemName} (x{$orderQuantity})
    {"[bold]Requested by:[/bold]"} {$requester}

    {"[head=3]Order Information[/head]"}
    {"[bold]Payer[/bold]:"} {$account} [font="Monospace"]\[{$accountcode}\][/font]
    {"[bold]Approved by:[/bold]"} {$approver}
    {"[bold]Reason:[/bold]"} {$reason}

# Cargo shuttle console
cargo-shuttle-console-menu-title = Консоль вантажного шаттла
cargo-shuttle-console-station-unknown = Невідомо
cargo-shuttle-console-shuttle-not-found = Не знайдено
cargo-shuttle-console-organics = Виявлено органічні форми життя на шатлі
cargo-no-shuttle = Вантажний шатл не знайдено!

# Funding allocation console
cargo-funding-alloc-console-menu-title = Консоль розподілу фінансування
cargo-funding-alloc-console-label-account = [bold]Обліковий запис[/bold]
cargo-funding-alloc-console-label-code = [bold]Код [/bold]
cargo-funding-alloc-console-label-balance = [bold]Баланс [/bold]
cargo-funding-alloc-console-label-cut = [bold]Відділ доходів (%) [/bold]

cargo-funding-alloc-console-label-primary-cut = Зменшення коштів на вантажі з інших джерел (%):
cargo-funding-alloc-console-label-lockbox-cut = Відрізок вантажів від продажу скриньок (%):

cargo-funding-alloc-console-label-help-non-adjustible = Cargo отримує {$percent}% прибутку від продажів без скриньок. Решта розподіляється, як зазначено нижче:
cargo-funding-alloc-console-label-help-adjustible = Залишок коштів із інших джерел розподіляється, як зазначено нижче:
cargo-funding-alloc-console-button-save = Зберегти зміни
cargo-funding-alloc-console-label-save-fail = [bold]Поділ доходу недійсний![/bold][color=red]({$pos ->
    [1] +
    *[-1] -
}{$val}%)[/color]

# Slip template
cargo-acquisition-slip-body = [head=3]Деталі активу[/head]
    {"[bold]Product:[/bold]"} {$product}
    {"[bold]Description:[/bold]"} {$description}
    {"[bold]Unit cost:[/bold"}] ${$unit}
    {"[bold]Amount:[/bold]"} {$amount}
    {"[bold]Cost:[/bold]"} ${$cost}

    {"[head=3]Purchase Detail[/head]"}
    {"[bold]Orderer:[/bold]"} {$orderer}
    {"[bold]Reason:[/bold]"} {$reason}