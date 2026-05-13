### Examine

gas-turbine-examine-stator-null = Здається, не вистачає статора.
gas-turbine-examine-stator = Має статор.

gas-turbine-examine-blade-null = Здається, у нього відсутня лопатка турбіни.
gas-turbine-examine-blade = Має турбінну лопатку.

turbine-spinning-0 = Леза не обертаються.
turbine-spinning-1 = Леза повільно обертаються.
turbine-spinning-2 = Леза крутяться.
turbine-spinning-3 = Лопаті швидко обертаються.
turbine-spinning-4 = [color=red]Леза виходять з-під контролю![/color]

turbine-damaged-0 = Здається, він у хорошому стані.[/color]
turbine-damaged-1 = Турбіна виглядає трохи потертою.[/color]
turbine-damaged-2 = [color=yellow]Турбіна виглядає дуже пошкодженою.[/color]
turbine-damaged-3 = [color=orange]Це серйозно пошкоджено![/color]

turbine-ruined = [color=red]Це повністю зламано![/color]

### Popups

# Shown when an event occurs
turbine-overheat = {$owner}запускає аварійний клапан скидання перегріву!
turbine-explode = {$owner}розривається!

# Shown when damage occurs
turbine-spark = {$owner}починає іскрити!
turbine-spark-stop = {$owner}перестає іскрити.
turbine-smoke = {$owner}починає диміти!
turbine-smoke-stop = {$owner}перестає палити.

# Shown during repairs
gas-turbine-repair-fail-blade = Вам потрібно замінити лопатку турбіни, перш ніж це можна буде відремонтувати.
gas-turbine-repair-fail-stator = Вам потрібно замінити статор, перш ніж це можна буде відремонтувати.
turbine-repair-ruined = Ви ремонтуєте корпус {$target}за допомогою {$tool}.
turbine-repair = Ви відновлюєте частину пошкоджень {$target}за допомогою {$tool}.
turbine-no-damage = Немає пошкоджень для ремонту на {$target}за допомогою {$tool}.
turbine-show-damage = BladeHealth {$health}, BladeHealthMax {$healthMax}.

# Anchoring warnings
turbine-unanchor-warning = Ви не можете розкріпити газову турбіну, поки турбіна обертається!
turbine-anchor-warning = Недійсне положення прив'язки.

gas-turbine-eject-fail-speed = Ви не можете знімати частини турбіни, поки турбіна обертається!
gas-turbine-insert-fail-speed = Не можна вставляти деталі турбіни, коли турбіна обертається!

### UI

# Shown when using the UI
comp-turbine-ui-tab-main = Елементи управління
comp-turbine-ui-tab-parts = Запчастини

comp-turbine-ui-rpm = RPM

comp-turbine-ui-overspeed = ПЕРЕВИЩЕННЯ ШВИДКОСТІ
comp-turbine-ui-overtemp = ПЕРЕТЕП
comp-turbine-ui-stalling = ЗВІТ
comp-turbine-ui-undertemp = НИЗЬКА ТЕМПЕРАТУРА

comp-turbine-ui-flow-rate = Швидкість потоку
comp-turbine-ui-stator-load = Навантаження статора

comp-turbine-ui-blade = Лопатка турбіни
comp-turbine-ui-blade-integrity = Цілісність
comp-turbine-ui-blade-stress = Стрес

comp-turbine-ui-stator = Статор турбіни
comp-turbine-ui-stator-potential = потенціал
comp-turbine-ui-stator-supply = Постачання

comp-turbine-ui-power = { POWERWATTS($power)}

comp-turbine-ui-locked-message = Елементи керування заблоковано.
comp-turbine-ui-footer-left = Небезпека: машини, що швидко рухаються.
comp-turbine-ui-footer-right = 2.0 REV 1