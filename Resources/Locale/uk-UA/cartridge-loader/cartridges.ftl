device-pda-slot-component-slot-name-cartridge = Картридж

default-program-name = програма
notekeeper-program-name = Заміки
nano-task-program-name = NanoTask
news-read-program-name = Новини станції

crew-manifest-program-name = Маніфест екіпажу
crew-manifest-cartridge-loading = Загрузка ...

net-probe-program-name = NetProbe
net-probe-scan = Скановано {$device}!
net-probe-label-name = Імʼя
net-probe-label-address = Адреса
net-probe-label-frequency = Частота
net-probe-label-network = Мережа

log-probe-program-name = LogProbe
log-probe-scan = Завантажено журнали з {$device}!
log-probe-label-time = час
log-probe-label-accessor = Доступ до
log-probe-label-number = #
log-probe-print-button = Друк журналів
log-probe-printout-device = Сканований пристрій: {$name}
log-probe-printout-header = Останні журнали:
log-probe-printout-entry = №{$number}/ {$time}/ {$accessor}

astro-nav-program-name = Кімнатна температура

med-tek-program-name = MedTek

# NanoTask cartridge

nano-task-ui-heading-high-priority-tasks =
    { $amount ->
        [zero] No High Priority Tasks
        [one] 1 High Priority Task
       *[other] {$amount} High Priority Tasks
    }
nano-task-ui-heading-medium-priority-tasks =
    { $amount ->
        [zero] No Medium Priority Tasks
        [one] 1 Medium Priority Task
       *[other] {$amount} Medium Priority Tasks
    }
nano-task-ui-heading-low-priority-tasks =
    { $amount ->
        [zero] No Low Priority Tasks
        [one] 1 Low Priority Task
       *[other] {$amount} Low Priority Tasks
    }
nano-task-ui-done = Готово
nano-task-ui-revert-done = Скасувати
nano-task-ui-priority-low = Низький
nano-task-ui-priority-medium = Середній
nano-task-ui-priority-high = Високий
nano-task-ui-cancel = Скасувати
nano-task-ui-print = Роздрукувати
nano-task-ui-delete = Видалити
nano-task-ui-save = Епістеміка
nano-task-ui-new-task = Нове завдання
nano-task-ui-description-label = опис:
nano-task-ui-description-placeholder = Отримати щось важливе
nano-task-ui-requester-label = Запитувач:
nano-task-ui-requester-placeholder = Джон Нанотрасен
nano-task-ui-item-title = Редагувати завдання
nano-task-printed-description = [bold]Опис[/bold]: {$description}
nano-task-printed-requester = [bold]Запитувач[/bold]: {$requester}
nano-task-printed-high-priority = [bold]Пріоритет[/bold]: [color=red]Високий[/color]
nano-task-printed-medium-priority = [bold]Пріоритет[/bold]: середній
nano-task-printed-low-priority = [bold]Пріоритет[/bold]: Низький

# Wanted list cartridge
wanted-list-program-name = Список розшуку
wanted-list-label-no-records = Все гаразд, ковбой
wanted-list-search-placeholder = Пошук за назвою та статусом

wanted-list-age-label = [color=darkgray]Вік:[/color][color=white]{$age}[/color]
wanted-list-job-label = [color=darkgray]Посада:[/color][color=white]{$job}[/color]
wanted-list-species-label = [color=darkgray]Види:[/color][color=white]{$species}[/color]
wanted-list-gender-label = [color=darkgray]Стать:[/color][color=white]{$gender}[/color]

wanted-list-reason-label = [color=darkgray]Причина:[/color][color=white]{$reason}[/color]
wanted-list-unknown-reason-label = невідома причина

wanted-list-initiator-label = [color=darkgray]Ініціатор:[/color][color=white]{$initiator}[/color]
wanted-list-unknown-initiator-label = невідомий ініціатор

wanted-list-status-label = [color=darkgray]статус:[/color]{$status ->
        [suspected] [color=yellow]suspected[/color]
        [wanted] [color=red]wanted[/color]
        [detained] [color=#b18644]detained[/color]
        [paroled] [color=green]paroled[/color]
        [discharged] [color=green]discharged[/color]
        [hostile] [color=darkred]hostile[/color]
        [eliminated] [color=gray]eliminated[/color]
        *[other] none
    }

wanted-list-history-table-time-col = час
wanted-list-history-table-reason-col = Злочинність
wanted-list-history-table-initiator-col = Ініціатор