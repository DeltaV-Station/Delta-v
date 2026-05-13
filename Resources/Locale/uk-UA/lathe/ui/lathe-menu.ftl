lathe-menu-title = Токарне меню
lathe-menu-queue = Черга
lathe-menu-server-list = Список серверів
lathe-menu-sync = Синхронізувати
lathe-menu-search-designs = Пошук дизайнів
lathe-menu-category-all = все
lathe-menu-search-filter = Фільтр:
lathe-menu-amount = сума:
lathe-menu-recipe-count = { $count ->
    [1] {$count} Recipe
    *[other] {$count} Recipes
}
lathe-menu-reagent-slot-examine = Збоку має проріз для склянки.
lathe-reagent-dispense-no-container = Рідина виливається з {THE($name)} на підлогу!
lathe-menu-result-reagent-display = {$reagent}({$amount}u)
lathe-menu-material-display = {$material}({$amount})
lathe-menu-tooltip-display = {$amount}з {$material}
lathe-menu-description-display = [italic]{$description}[/italic]
lathe-menu-material-amount = { $сума ->
    [1] {NATURALFIXED($amount, 2)} {$unit}
    *[other] {NATURALFIXED($amount, 2)} {MAKEPLURAL($unit)}
}
lathe-menu-material-amount-missing = { $сума ->
    [1] {NATURALFIXED($amount, 2)} {$unit} of {$material} ([color=red]{NATURALFIXED($missingAmount, 2)} {$unit} missing[/color])
    *[other] {NATURALFIXED($amount, 2)} {MAKEPLURAL($unit)} of {$material} ([color=red]{NATURALFIXED($missingAmount, 2)} {MAKEPLURAL($unit)} missing[/color])
}
lathe-menu-no-materials-message = Матеріали не завантажено.
lathe-menu-silo-linked-message = Silo Linked
lathe-menu-fabricating-message = Виготовлення...
lathe-menu-materials-title = Матеріали
lathe-menu-queue-title = Створення черги
lathe-menu-delete-fabricating-tooltip = Скасувати друк поточного елемента.
lathe-menu-delete-item-tooltip = Скасувати друк цієї партії.
lathe-menu-move-up-tooltip = Перемістіть цю партію вперед у черзі.
lathe-menu-move-down-tooltip = Перемістіть цю партію назад у чергу.
lathe-menu-item-single = {$index}. {$name}
lathe-menu-item-batch = {$index}. {$name}({$printed}/{$total})