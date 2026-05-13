# Mail-related commands

## mailto
cmd-mailto-hint-recipient = Recipient EntityUid (повинен мати MailReceiverComponent)
cmd-mailto-hint-container = Container EntityUid (сутність із вмістом для передачі)
cmd-mailto-hint-fragile = Крихкий? (правда/неправда)
cmd-mailto-hint-priority = Пріоритет? (правда/неправда)
cmd-mailto-hint-large = великий? (правда/неправда, необов'язково)
cmd-mailto-description = Поставте посилку в чергу для доставки до організації. Вміст цільового контейнера буде перенесено у справжню поштову посилку.
cmd-mailto-help = Використання: {$command}<recipient entityUid> <container entityUid> [is-fragile: true або false] [is-priority: true або false] [is-large: true або false, необов’язково]
cmd-mailto-no-mailreceiver = Цільовий одержувач не має {$requiredComponent}.
cmd-mailto-no-blankmail = Прототип {$blankMail}не існує. Something is very wrong. Contact a programmer.
cmd-mailto-bogus-mail = {$blankMail}не мав {$requiredMailComponent}. Щось дуже не так. Зверніться до програміста.
cmd-mailto-invalid-container = Об’єкт цільового контейнера не має контейнера {$requiredContainer}.
cmd-mailto-unable-to-receive = Об’єкт цільового одержувача не вдалося налаштувати для отримання пошти. ID може бути відсутнім.
cmd-mailto-no-teleporter-found = Об’єкт цільового одержувача не вдалося зіставити з поштовим телепортом жодної станції. Одержувач може бути поза станцією.
cmd-mailto-success = Успіх! Поштова посилка поставлена ​​в чергу для наступного телепорту через {$timeToTeleport}секунд.

## mailnow
cmd-mailnow = Змусити всіх поштових телепортаторів доставити ще одну партію пошти якомога швидше. Це не обійде ліміт недоставленої пошти.
cmd-mailnow-help = Використання: {$command}
cmd-mailnow-success = Успіх! Усі поштові телепортатори незабаром доставлять ще одну партію пошти.