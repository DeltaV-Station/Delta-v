server-role-ban =
    Временный джоб-бан на { $mins } { $mins ->
        [one] минуту
        [few] минуты
       *[other] минут
    }.
server-perma-role-ban = Перманентный джоб-бан.
server-time-ban-string =
    > **Нарушитель**
        > **Логин:** ``{ $targetName }``
    
        > **Администратор**
        > **Логин:** ``{ $adminName }``
    
        > **Выдан:** { $TimeNow }
        > **Истечет:** { $expiresString }
    
        > **Причина:** { $reason }
server-ban-footer = { $server } | Раунд: #{ $round }
server-perma-ban-string =
    > **Нарушитель**
        > **Логин:** ``{ $targetName }``
    
        > **Администратор**
        > **Логин:** ``{ $adminName }``
    
        > **Выдан:** { $TimeNow }
    
        > **Причина:** { $reason }
server-role-ban-string =
    > **Нарушитель**
        > **Логин:** ``{ $targetName }``
    
        > **Администратор**
        > **Логин:** ``{ $adminName }``
    
        > **Выдан:** { $TimeNow }
        > **Истечет:** { $expiresString }
    
        > **Роли:** { $roles }
    
        > **Причина:** { $reason }
server-perma-role-ban-string =
    > **Нарушитель**
        > **Логин:** ``{ $targetName }``
    
        > **Администратор**
        > **Логин:** ``{ $adminName }``
    
        > **Выдан:** { $TimeNow }
    
        > **Роли:** { $roles }
    
        > **Причина:** { $reason }
server-ban-string-infinity = Вечно
server-ban-no-name = Не найдено. ({ $hwid })
server-time-ban =
    Временный бан на { $mins } { $mins ->
        [one] минуту
        [few] минуты
       *[other] минут
    }.
server-perma-ban = Перманентный бан.
