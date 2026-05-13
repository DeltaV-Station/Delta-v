cmd-atvrange-desc = Встановлює діапазон налагодження atmos (як два числа з плаваючою точкою, початок [red]і кінець [blue])
cmd-atvrange-help = Використання: {$command}<початок> <кінець>
cmd-atvrange-error-start = Поганий поплавковий СТАРТ
cmd-atvrange-error-end = Поганий float END
cmd-atvrange-error-zero = Масштаб не може дорівнювати нулю, оскільки це призведе до ділення на нуль у AtmosDebugOverlay.

cmd-atvmode-desc = Встановлює режим налагодження atmos. Це призведе до автоматичного скидання ваги.
cmd-atvmode-help = Використання: {$command}<TotalMoles/GasMoles/Temperature> [<ідентифікатор газу (для GasMoles)>]
cmd-atvmode-error-invalid = Недійсний режим
cmd-atvmode-error-target-gas = Для цього режиму необхідно забезпечити цільовий газ.
cmd-atvmode-error-out-of-range = Ідентифікатор газу не аналізується або виходить за межі діапазону.
cmd-atvmode-error-info = Додаткова інформація для цього режиму не потрібна.

cmd-atvcbm-desc = Зміна від червоного/зеленого/синього до відтінків сірого
cmd-atvcbm-help = Використання: {$command}<true/false>
cmd-atvcbm-error = Недійсний прапор