### for technical and/or system messages

## General

shell-command-success = Команда успішна
shell-invalid-command = Невірна команда.
shell-invalid-command-specific = Обгортка
shell-can-only-run-from-pre-round-lobby = Ви можете запустити цю команду, лише коли гра триває у передраундовому лобі.
shell-can-only-run-while-round-is-active = Ви можете запустити цю команду лише під час раунду гри.
shell-cannot-run-command-from-server = Ви не можете запустити цю команду з сервера.
shell-only-players-can-run-this-command = Тільки гравці можуть робити цю команду.
shell-must-be-attached-to-entity = Ви повинні бути приєднані до сутності, щоб запустити цю команду.
shell-must-have-body = Ви повинні мати тіло, щоб виконати цю команду.

## Arguments

shell-need-exactly-one-argument = Потрібно рівно 1 аргумент (параметр).
shell-wrong-arguments-number-need-specific = Потрібно {$properAmount} аргументів, а було надано {$currentAmount}.
shell-argument-must-be-number = аргумент має бути числом.
shell-argument-must-be-boolean = аргумент має бути true/false.
shell-wrong-arguments-number = Невірна кількість аргументів.
shell-need-between-arguments = Потрібно від {$lower}до {$upper}аргументів!
shell-need-minimum-arguments = Потрібно щонайменше {$minimum}аргументів!
shell-need-minimum-one-argument = Потрібно хоча б 1 аргумент!
shell-need-exactly-zero-arguments = Ця команда приймає нуль аргументів.

shell-argument-uid = PB-[[0]]

## Guards

shell-missing-required-permission = Вам потрібно {$perm}для цієї команди!
shell-entity-is-not-mob = Цільова сутність не є натовпом!
shell-invalid-entity-id = Недійсний ідентифікатор організації.
shell-invalid-grid-id = Недійсний ідентифікатор сітки.
shell-invalid-map-id = Недійсний ідентифікатор карти.
shell-invalid-entity-uid = {$uid}не є дійсним uid об’єкта
shell-invalid-bool = Недійсне логічне значення.
shell-entity-uid-must-be-number = EntityUid має бути числом.
shell-could-not-find-entity = Не вдалося знайти сутність {$entity}
shell-could-not-find-entity-with-uid = Не вдалося знайти сутність з uid {$uid}
shell-entity-with-uid-lacks-component = Сутність з uid {$uid}не має компонента {INDEFINITE($componentName)} {$componentName}
shell-entity-target-lacks-component = Цільова сутність не має компонента {INDEFINITE($componentName)} {$componentName}
shell-invalid-color-hex = Недійсний шістнадцятковий колір!
shell-target-player-does-not-exist = Цільового гравця не існує!
shell-target-entity-does-not-have-message = Цільова сутність не має {INDFINITE($missing)} {$missing}!
shell-timespan-minutes-must-be-correct = {$span}не є дійсним проміжком часу в хвилинах.
shell-argument-must-be-prototype = Аргумент {$index}має бути {LOC($prototypeName)}!
shell-argument-number-must-be-between = Аргумент {$index}має бути числом між {$lower}і {$upper}!
shell-argument-station-id-invalid = Аргумент {$index}має бути дійсним ідентифікатором станції!
shell-argument-map-id-invalid = Аргумент {$index}має бути дійсним ідентифікатором карти!
shell-argument-number-invalid = Аргумент {$index}має бути дійсним числом!

# Hints
shell-argument-username-hint = [[0]] додано до чорного списку
shell-argument-username-optional-hint = [username]