# ban
cmd-ban-desc = Забанити когось
cmd-ban-help = Використання: бан <ім'я або ідентифікатор користувача> <причина> [тривалість у хвилинах, пропустіть або 0 для постійного блокування]
cmd-ban-player = Не вдалося знайти гравця з таким іменем.
cmd-ban-invalid-minutes = як злегка солодкі алкогольні фрукти
cmd-ban-invalid-severity = {$severity}не є дійсним рівнем серйозності!
cmd-ban-invalid-arguments = Неправильна кількість аргументів
cmd-ban-hint = <ім'я/ідентифікатор користувача>
cmd-ban-hint-reason = плитка xenoborg
cmd-ban-hint-duration = [duration]
cmd-ban-hint-severity = [severity]

cmd-ban-hint-duration-1 = Назавжди
cmd-ban-hint-duration-2 = 1 день
cmd-ban-hint-duration-3 = 3 дні
cmd-ban-hint-duration-4 = 1 тиждень
cmd-ban-hint-duration-5 = 2 тижні
cmd-ban-hint-duration-6 = 1 місяць

# ban panel
cmd-banpanel-desc = Відкриває панель заборон
cmd-banpanel-help = Використання: banpanel [name or user guid]
cmd-banpanel-server = Це не можна використовувати з консолі сервера
cmd-banpanel-player-err = Не вдалося знайти вказаного гравця

# listbans
cmd-banlist-desc = Перелічує активні заборони користувача.
cmd-banlist-help = Використання: banlist <ім'я або ідентифікатор користувача>
cmd-banlist-empty = Не знайдено активних заборон для {$user}
cmd-banlist-hint = <ім'я/ідентифікатор користувача>

cmd-ban_exemption_update-desc = Встановіть виняток для типу заборони для гравця.
cmd-ban_exemption_update-help = Використання: ban_exemption_update <гравець> <прапор> [<прапор> [...]]
    Specify multiple flags to give a player multiple ban exemption flags.
    To remove all exemptions, run this command and give "None" as only flag.

cmd-ban_exemption_update-nargs = Очікується щонайменше 2 аргументи
cmd-ban_exemption_update-locate = Неможливо знайти гравця '{$player}'.
cmd-ban_exemption_update-invalid-flag = Недійсний прапор "{$flag}".
cmd-ban_exemption_update-success = Оновлено прапорці звільнення від заборони для '{$player}' ({$uid}).
cmd-ban_exemption_update-arg-player = <гравець>
cmd-ban_exemption_update-arg-flag = <прапор>

cmd-ban_exemption_get-desc = Показати винятки з заборони для певного гравця.
cmd-ban_exemption_get-help = Використання: ban_exemption_get <гравець>

cmd-ban_exemption_get-nargs = Очікується рівно 1 аргумент
cmd-ban_exemption_get-none = Користувач не звільняється від жодних заборон.
cmd-ban_exemption_get-show = Користувач звільнений від таких позначок заборони: {$flags}.
cmd-ban_exemption_get-arg-player = <гравець>

# Ban panel
ban-panel-title = Панель заборони
ban-panel-player = гравець
ban-panel-ip = IP
ban-panel-hwid = HWID
ban-panel-reason = Причина
ban-panel-last-conn = Використовувати IP та HWID з останнього підключення?
ban-panel-submit = Забанити
ban-panel-confirm = Ви впевнені?
ban-panel-tabs-basic = Основна інформація
ban-panel-tabs-reason = Причина
ban-panel-tabs-players = Список гравців
ban-panel-tabs-role = мислення
ban-panel-no-data = Ви повинні вказати користувача, IP або HWID для заборони
ban-panel-invalid-ip = Не вдалося проаналізувати IP-адресу. Спробуйте ще раз
ban-panel-select = Виберіть тип
ban-panel-server = Бан сервера
ban-panel-role = Заборона ролі
ban-panel-minutes = хвилин
ban-panel-hours = години
ban-panel-days = днів
ban-panel-weeks = тижнів
ban-panel-months = Місяці
ban-panel-years = років
ban-panel-permanent = Постійний
ban-panel-ip-hwid-tooltip = Залиште порожнім і поставте прапорець нижче, щоб використовувати деталі останнього підключення
ban-panel-severity = Серйозність:
ban-panel-erase = Видалити повідомлення чату та гравця з раунду
ban-panel-expiry-error = помилка

# Ban string
server-ban-string = {$admin}створив {$severity}бан на сервері, термін дії якого закінчується {$expires}для [{$name}, {$ip}, {$hwid}], з причиною: {$reason}
server-ban-string-no-pii = {$admin}створив заборону сервера {$severity}, термін дії якої закінчується {$expires}для {$name}з причиною: {$reason}
server-ban-string-never = ніколи

# Kick on ban
ban-kick-reason = Вас забанили