## Strings for the "grant_connect_bypass" command.

cmd-grant_connect_bypass-desc = Тимчасово дозволити користувачеві обійти регулярні перевірки підключення.
cmd-grant_connect_bypass-help = Використання: grant_connect_bypass <користувач> [duration minutes]
    Temporarily grants a user the ability to bypass regular connections restrictions.
    The bypass only applies to this game server and will expire after (by default) 1 hour.
    They will be able to join regardless of whitelist, panic bunker, or player cap.

cmd-grant_connect_bypass-arg-user = <користувач>
cmd-grant_connect_bypass-arg-duration = [duration minutes]

cmd-grant_connect_bypass-invalid-args = Очікується 1 або 2 аргументи
cmd-grant_connect_bypass-unknown-user = Не вдалося знайти користувача "{$user}"
cmd-grant_connect_bypass-invalid-duration = Недійсна тривалість '{$duration}'

cmd-grant_connect_bypass-success = Успішно додано обхід для користувача "{$user}"