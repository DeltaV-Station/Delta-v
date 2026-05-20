# Тестовий файл для перевірки українських функцій

# Тест UKPLURAL
test-plural-1 = Знайдено { $count } { UKPLURAL($count, "предмет", "предмети", "предметів") }
test-plural-2 = Вбито { $count } { UKPLURAL($count, "ворог", "вороги", "ворогів") }

# Тест UKGENDER
test-gender = { $name } { UKGENDER($gender, "приєднався", "приєдналася", "приєдналося") } до гри

# Тест UKCASE
test-case-dative = Передати { UKCASE("dative", "гравець", "гравця", "гравцю", "гравця", "гравцем", "гравцю") }
test-case-genitive = Інвентар { UKCASE("genitive", "гравець", "гравця", "гравцю", "гравця", "гравцем", "гравцю") }

# Тест UKPLURALGEN
test-pluralgen = { $count } { UKPLURAL($count, "гравець", "гравці", "гравців") } { UKPLURALGEN($count, $gender, "взяв/взяла/взяло", "взяли/взяли/взяли", "взяли/взяли/взяли") } предмет

# Тест UKTIME
test-time = Час гри: { UKTIME($hours, $minutes) }

# Тест UKLIST
test-list-2 = Доступні предмети: { UKLIST("яблуко", "груша") }
test-list-3 = Доступні предмети: { UKLIST("яблуко", "груша", "банан") }
test-list-4 = Доступні предмети: { UKLIST("яблуко", "груша", "банан", "апельсин") }

# Тест UKNAME - автоматичне відмінювання імен
test-name-hug = { UKNAME("nominative", $user) } обійняв { UKNAME("accusative", $target) }
test-name-give = { UKNAME("nominative", $user) } передав предмет { UKNAME("dative", $target) }
test-name-hit = { UKNAME("nominative", $user) } вдарив { UKNAME("accusative", $target) }
test-name-about = Інформація про { UKNAME("accusative", $name) }

# Приклади:
# Іван обійняв Марію
# Петро передав предмет Софії
# Олександр вдарив Івана
# Інформація про Марію
