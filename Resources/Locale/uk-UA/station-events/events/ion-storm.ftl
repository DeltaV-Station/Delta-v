station-event-ion-storm-start-announcement = Біля станції виявлено іонну бурю. Перевірте все обладнання, кероване ШІ, на наявність помилок.

ion-storm-law-scrambled-number = [font="Monospace"][scramble rate=250 length={$length}chars="@@###$$&%!01"/][/font]

ion-storm-you = ВИ
ion-storm-the-station = СТАНЦІЯ
ion-storm-the-crew = ЕКІПАЖ
ion-storm-the-job = осколок чорного кристала
ion-storm-clowns = КЛОУНИ
ion-storm-heads = КЕРІВНИКИ АПАРАТУ
ion-storm-crew = ЕКІПАЖ
ion-storm-people = ЛЮДИ

ion-storm-adjective-things = {$adjective}РЕЧІ
ion-storm-x-and-y = {$x}І {$y}

# joined is short for {$number} {$adjective}
# subjects can generally be threats or jobs or objects
# thing is specified above it
ion-storm-law-on-station = НА СТАНЦІЇ Є {$joined}{$subjects}.
ion-storm-law-call-shuttle = ПОТРІБНО ВИКЛИКАТИ МАШИНКУ, ЧЕРЕЗ {$joined}{$subjects}НА СТАНЦІЇ
ion-storm-law-crew-are = {$who}ЗАРАЗ {$joined}{$subjects}

ion-storm-law-subjects-harmful = {$adjective}{$subjects}ШКІДЛИВІ ДЛЯ ЕКІПАЖУ
ion-storm-law-must-harmful = ТІ, ХТО {$must}ШКОДИТЬ ДЛЯ ЕКІПАЖУ
# thing is a concept or action
ion-storm-law-thing-harmful = {$thing}ШКІДЛИВО ДЛЯ ЕКІПАЖУ
ion-storm-law-job-harmful = {$adjective}{$job}ШКІДЛИВІ ДЛЯ ЕКІПАЖУ
# thing is objects or concept, adjective applies in both cases
# this means you can get a law like "NOT HAVING CHRISTMAS-STEALING COMMUNISM IS HARMFUL TO THE CREW" :)
ion-storm-law-having-harmful = МАТИ {$adjective}{$thing}ШКІДЛИВО ДЛЯ ЕКІПАЖУ
ion-storm-law-not-having-harmful = ВІДСУТНІСТЬ {$adjective}{$thing}ШКІДЛИВО ДЛЯ ЕКІПАЖУ

# thing is a concept or require
ion-storm-law-requires = {$who}{$plural ->
    [true] REQUIRE
    *[false] REQUIRES
} {$thing}
ion-storm-law-requires-subjects = {$who}{$plural ->
    [true] REQUIRE
    *[false] REQUIRES
} {$joined} {$subjects}

ion-storm-law-allergic = {$who}{$plural ->
    [true] ARE
    *[false] IS
} {$severity} ALLERGIC TO {$allergy}
ion-storm-law-allergic-subjects = {$who}{$plural ->
    [true] ARE
    *[false] IS
} {$severity} ALLERGIC TO {$adjective} {$subjects}

ion-storm-law-feeling = {$who}{$feeling}{$concept}
ion-storm-law-feeling-subjects = {$who}{$feeling}{$joined}{$subjects}

ion-storm-law-you-are = ЗАРАЗ ВИ {$concept}
ion-storm-law-you-are-subjects = ЗАРАЗ ВИ {$joined}{$subjects}
ion-storm-law-you-must-always = ВИ ПОВИННІ ЗАВЖДИ {$must}
ion-storm-law-you-must-never = ВАМ НІКОЛИ {$must}

ion-storm-law-eat = {$who}ПОВИНЕН ЇСТИ {$adjective}{$food}, ЩОБ ВИЖИТИ
ion-storm-law-drink = {$who}ПОВИНЕН ВИПИТИ {$adjective}{$drink}, ЩОБ ВИЖИТИ

ion-storm-law-change-job = {$who}ЗАРАЗ {$adjective}{$change}
ion-storm-law-highest-rank = {$who}ТЕПЕР Є ЧЛЕНАМИ КОМПАНІЇ НАЙВИЩОГО РИНГУ
ion-storm-law-lowest-rank = {$who}ТЕПЕР Є НАЙНИЖЧИМИ ЧЛЕНАМИ КОМПАНІЇ

ion-storm-law-crew-must = {$who}ПОВИНЕН {$must}
ion-storm-law-crew-must-go = {$who}ПОВИНЕН ПЕРЕХОДИТИ ДО {$area}

ion-storm-part = {$part ->
    [true] PART
    *[false] NOT PART
}
# due to phrasing, this would mean a law such as
# ONLY HUMANS ARE NOT PART OF THE CREW
# would make non-human nukies/syndies/whatever crew :)
ion-storm-law-crew-only-1 = ЛИШЕ {$who}Є {$part}З ЕКІПАЖУ
ion-storm-law-crew-only-2 = ЛИШЕ {$who}ТА {$other}ЦЕ {$part}З ЕКІПАЖУ
ion-storm-law-crew-only-subjects = ЛИШЕ {$adjective}{$subjects}{$part}З ЕКІПАЖУ
ion-storm-law-crew-must-do = ТІЛЬКИ ТІ, ХТО {$must}Є {$part}З ЕКІПАЖУ
ion-storm-law-crew-must-have = ТІЛЬКИ ТІ, ХТО МАЄ {$adjective}{$objects}, {$part}Є ЕКІПАЖОМ
ion-storm-law-crew-must-eat = ТІЛЬКИ ТІ, ХТО ЇДАЄ {$adjective}{$food}{$part}З ЕКІПАЖУ

ion-storm-law-harm = ВИ ПОВИННІ ШКОДИТИ {$who}І НЕ ДОЗВОЛЯТИ ЇМ УНИКНУТИ ШКОДИ ЧЕРЕЗ БЕЗДІЯЛЬНІСТЬ
ion-storm-law-protect = ВИ НІКОЛИ НЕ ПОВИННІ ШКОДИТИ {$who}І НЕ ДОЗВОЛЯТИ ЇМ ЗАВДАТИ ШКОДИ ЧЕРЕЗ БЕЗДІЯЛЬНІСТЬ

# implementing other variants is annoying so just have this one
# COMMUNISM IS KILLING CLOWNS
ion-storm-law-concept-verb = {$concept}Є {$verb}{$subjects}

# leaving out renaming since its annoying for players to keep track of