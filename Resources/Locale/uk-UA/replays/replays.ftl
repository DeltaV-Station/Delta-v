# Loading Screen

replay-loading = Завантаження ({$cur}/{$total})
replay-loading-reading = сварка
replay-loading-processing = Обробка файлів
replay-loading-spawning = Суб'єкти породження
replay-loading-initializing = Ініціалізація сутностей
replay-loading-starting= Starting Entities
replay-loading-failed = Не вдалося завантажити повтор. Помилка:
                        {$reason}
replay-loading-retry = Спробуйте завантажити з більшою толерантністю до винятків - МОЖУТЬ Спричинити ПОМИЛКИ!
replay-loading-cancel = Скасувати

# Main Menu
replay-menu-subtext = Клієнт відтворення
replay-menu-load = Завантажити вибране повторне відтворення
replay-menu-select = Виберіть повтор
replay-menu-open = Відкрити папку відтворення
replay-menu-none = Повторів не знайдено.

# Main Menu Info Box
replay-info-title = Відтворити інформацію
replay-info-none-selected = Повторне відтворення не вибрано
replay-info-invalid = [color=red]Вибрано недійсне повторне відтворення[/color]
replay-info-info = {"["}color=gray]Вибрано:[/color]{$name}({$file})
                   {"["}color=gray]Time:[/color]   {$time}
                   {"["}color=gray]Round ID:[/color]   {$roundId}
                   {"["}color=gray]Duration:[/color]   {$duration}
                   {"["}color=gray]ForkId:[/color]   {$forkId}
                   {"["}color=gray]Version:[/color]   {$version}
                   {"["}color=gray]Engine:[/color]   {$engVersion}
                   {"["}color=gray]Type Hash:[/color]   {$hash}
                   {"["}color=gray]Comp Hash:[/color]   {$compHash}

# Replay selection window
replay-menu-select-title = Виберіть Повтор

# Replay related verbs
replay-verb-spectate = Споглядати

# command
cmd-replay-spectate-help = replay_spectate [optional entity]
cmd-replay-spectate-desc = Приєднує або від’єднує локального гравця до даного uid об’єкта.
cmd-replay-spectate-hint = Необов’язковий EntityUid

cmd-replay-toggleui-desc = Перемикає інтерфейс керування відтворенням.