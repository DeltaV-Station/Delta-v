parse-minutes-fail = Не вдалося розібрати '{$minutes}' як хвилини
parse-session-fail = Не знайдено сеанс для '{$username}'

## Role Timer Commands

# - playtime_addoverall
cmd-playtime_addoverall-desc = Додає вказані хвилини до загального часу гри гравця
cmd-playtime_addoverall-help = Використання: {$command}<ім'я користувача> <хвилини>
cmd-playtime_addoverall-succeed = Загальний час для {$username}збільшено до {TOSTRING($time, "dddd\\:hh\\:mm")}
cmd-playtime_addoverall-arg-user = <ім'я користувача>
cmd-playtime_addoverall-arg-minutes = <хвилин>
cmd-playtime_addoverall-error-args = Нуні

# - playtime_addrole
cmd-playtime_addrole-desc = Додає вказані хвилини до часу рольової гри гравця
cmd-playtime_addrole-help = Використання: {$command}<ім’я користувача> <роль> <хвилини>
cmd-playtime_addrole-succeed = Збільшено час рольової гри для {$username}/ \'{$role}\' до {TOSTRING($time, "dddd\\:hh\\:mm")}
cmd-playtime_addrole-arg-user = <ім'я користувача>
cmd-playtime_addrole-arg-role = <роль>
cmd-playtime_addrole-arg-minutes = <хвилин>
cmd-playtime_addrole-error-args = Очікується рівно три аргументи

# - playtime_getoverall
cmd-playtime_getoverall-desc = Отримує вказані хвилини для загального ігрового часу гравця
cmd-playtime_getoverall-help = Використання: {$command}<ім'я користувача>
cmd-playtime_getoverall-success = Загальний час для {$username}становить {TOSTRING($time, "dddd\\:hh\\:mm")}.
cmd-playtime_getoverall-arg-user = <ім'я користувача>
cmd-playtime_getoverall-error-args = Очікувався рівно один аргумент

# - GetRoleTimer
cmd-playtime_getrole-desc = Отримує всі або одну роль таймерів від гравця
cmd-playtime_getrole-help = Використання: {$command}<ім'я користувача> [role]
cmd-playtime_getrole-no = Таймерів ролей не знайдено
cmd-playtime_getrole-role = Роль: {$role}, Час гри: {$time}
cmd-playtime_getrole-overall = Загальний час відтворення {$time}
cmd-playtime_getrole-succeed = Час відтворення для {$username}: {TOSTRING($time, "dddd\\:hh\\:mm")}.
cmd-playtime_getrole-arg-user = <ім'я користувача>
cmd-playtime_getrole-arg-role = <роль|'Загальний'>
cmd-playtime_getrole-error-args = Очікується рівно один-два аргументи

# - playtime_save
cmd-playtime_save-desc = Зберігає час гри гравця в БД
cmd-playtime_save-help = Використання: {$command}<ім'я користувача>
cmd-playtime_save-succeed = Запуск таймера
cmd-playtime_save-arg-user = <ім'я користувача>
cmd-playtime_save-error-args = Очікувався рівно один аргумент

## 'playtime_flush' command'

cmd-playtime_flush-desc = Очистити активні трекери до збережених у відстеженні часу відтворення.
cmd-playtime_flush-help = Використання: {$command}[user name]
    This causes a flush to the internal storage only, it does not flush to DB immediately.
    If a user is provided, only that user is flushed.

cmd-playtime_flush-error-args = Очікується нуль або один аргумент
cmd-playtime_flush-arg-user = [user name]