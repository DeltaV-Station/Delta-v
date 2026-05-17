parse-session-fail = Не знайдено сесію для '{$username}'

cmd-playtime_addoverall-desc = Додає вказані хвилини до загального ігрового часу гравця

cmd-playtime_addoverall-help = Використання: {$command} <ім'я користувача> <хвилини>

cmd-playtime_addoverall-succeed = Збільшено загальний час для {$username} до {TOSTRING($time, "dddd\\:hh\\:mm")}

cmd-playtime_addoverall-arg-user = <ім'я користувача

cmd-playtime_addoverall-arg-minutes = <хвилини>

cmd-playtime_addoverall-error-args = Очікувані рівно два аргументи

cmd-playtime_addrole-desc = Додає вказані хвилини до часу рольової гри гравця

cmd-playtime_addrole-help = Використання: {$command} <ім'я користувача> <роль> <хвилини>

cmd-playtime_addrole-succeed = Збільшено час гри для {$username} / \'{$role}\' до {TOSTRING($time, "dddd\\:hh\\:mm")}

cmd-playtime_addrole-arg-user = <ім'я користувача

cmd-playtime_addrole-arg-role = <role>

cmd-playtime_addrole-arg-minutes = <хвилини>

cmd-playtime_addrole-error-args = Очікувані рівно три аргументи

cmd-playtime_getoverall-desc = Отримує вказані хвилини для загального ігрового часу гравця

cmd-playtime_getoverall-help = Використання: {$command} <ім'я користувача>

cmd-playtime_getoverall-success = Загальний час для {$username} дорівнює {TOSTRING($time, "dddd\\:hh\\:mm")}.

cmd-playtime_getoverall-arg-user = <ім'я користувача

cmd-playtime_getoverall-error-args = Очікувався рівно один аргумент

cmd-playtime_getrole-desc = Отримує всі або один таймер ролі від гравця

cmd-playtime_getrole-help = Використання: {$command} <ім'я користувача> [роль]

cmd-playtime_getrole-no = Не знайдено таймерів ролей

cmd-playtime_getrole-role = Роль: {$role}, Час гри: {$time}

cmd-playtime_getrole-overall = Загальний час гри становить {$time}

cmd-playtime_getrole-succeed = Час гри для {$username} є: {TOSTRING($time, "dddd\\:hh\\:mm")}.

cmd-playtime_getrole-arg-user = <ім'я користувача

cmd-playtime_getrole-arg-role = <role|'Overall'>

cmd-playtime_getrole-error-args = Очікується рівно один-два аргументи

cmd-playtime_save-desc = Зберігає час гри гравця в БД

cmd-playtime_save-help = Використання: {$command} <ім'я користувача>

cmd-playtime_save-succeed = Збережено ігровий час для {$username}

cmd-playtime_save-arg-user = <ім'я користувача

cmd-playtime_save-error-args = Очікувався рівно один аргумент

cmd-playtime_flush-desc = Змити активні трекери до збережених у відстеженні ігрового часу.

cmd-playtime_flush-help = Використання: {$command} [ім'я користувача]
    Це викликає збереження лише у внутрішнє сховище, а не безпосередньо в БД.
    Якщо вказано користувача, скидається тільки цей користувач.

cmd-playtime_flush-error-args = Очікувані нуль або один аргумент

cmd-playtime_flush-arg-user = [ім'я користувача]
