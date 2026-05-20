-створює-3rd-person =
    { $chance ->
        [1] Створює
        *[other] створює
    }

-спричиняє-3rd-person =
    { $chance ->
        [1] Спричиняє
        *[other] спричиняє
    }

-насичує-3rd-person =
    { $chance ->
        [1] Насичує
        *[other] насичує
    }

entity-effect-guidebook-spawn-entity =
    { $chance ->
        [1] Створює
        *[other] створює
    } { $amount ->
        [1] {INDEFINITE($entname)}
        *[other] {$amount} {MAKEPLURAL($entname)}
    }

entity-effect-guidebook-знищує =
    { $chance ->
        [1] Знищує
        *[other] знищує
    } об'єкт

entity-effect-guidebook-ламає =
    { $chance ->
        [1] Ламає
        *[other] ламає
    } об'єкт

entity-effect-guidebook-explosion =
    { $chance ->
        [1] Спричиняє
        *[other] спричиняє
    } вибух

entity-effect-guidebook-emp =
    { $chance ->
        [1] Спричиняє
        *[other] спричиняє
    } електромагнітний імпульс

entity-effect-guidebook-flash =
    { $chance ->
        [1] Спричиняє
        *[other] спричиняє
    } сліпучий спалах

entity-effect-guidebook-foam-area =
    { $chance ->
        [1] Створює
        *[other] створює
    } велику кількість піни

entity-effect-guidebook-smoke-area =
    { $chance ->
        [1] Створює
        *[other] створює
    } велику кількість диму

entity-effect-guidebook-насичує-thirst =
    { $chance ->
        [1] Насичує
        *[other] насичує
    } { $relative ->
        [1] спрагу середньо
        *[other] спрагу на {NATURALFIXED($relative, 3)}x середню швидкість
    }

entity-effect-guidebook-насичує-hunger =
    { $chance ->
        [1] Насичує
        *[other] насичує
    } { $relative ->
        [1] голод середньо
        *[other] голод на {NATURALFIXED($relative, 3)}x середню швидкість
    }

entity-effect-guidebook-лікуєth-change =
    { $chance ->
        [1] { $лікуєorзавдає ->
                [лікує] Лікує
                [завдає] Завдає
                *[both] Змінює здоров'я на
             }
        *[other] { $лікуєorзавдає ->
                    [лікує] лікує
                    [завдає] завдає
                    *[both] змінює здоров'я на
                 }
    } { $changes }

entity-effect-guidebook-even-лікуєth-change =
    { $chance ->
        [1] { $лікуєorзавдає ->
            [лікує] Рівномірно лікує
            [завдає] Рівномірно завдає
            *[both] Рівномірно змінює здоров'я на
        }
        *[other] { $лікуєorзавдає ->
            [лікує] рівномірно лікує
            [завдає] рівномірно завдає
            *[both] рівномірно змінює здоров'я на
        }
    } { $changes }

entity-effect-guidebook-status-effect-old =
    { $type ->
        [update]{ $chance ->
                    [1] Спричиняє
                     *[other] спричиняє
                 } {LOC($key)} принаймні на {NATURALFIXED($time, 3)} {MANY("секунду", $time)} без накопичення
        [додає]   { $chance ->
                    [1] Спричиняє
                    *[other] спричиняє
                } {LOC($key)} принаймні на {NATURALFIXED($time, 3)} {MANY("секунду", $time)} з накопиченням
        [встановлює]  { $chance ->
                    [1] Спричиняє
                    *[other] спричиняє
                } {LOC($key)} на {NATURALFIXED($time, 3)} {MANY("секунду", $time)} без накопичення
        *[видаляє]{ $chance ->
                    [1] Видаляє
                    *[other] видаляє
                } {NATURALFIXED($time, 3)} {MANY("секунду", $time)} з {LOC($key)}
    }

entity-effect-guidebook-status-effect =
    { $type ->
        [update]{ $chance ->
                    [1] Спричиняє
                    *[other] спричиняє
                 } {LOC($key)} принаймні на {NATURALFIXED($time, 3)} {MANY("секунду", $time)} без накопичення
        [додає]   { $chance ->
                    [1] Спричиняє
                    *[other] спричиняє
                } {LOC($key)} принаймні на {NATURALFIXED($time, 3)} {MANY("секунду", $time)} з накопиченням
        [встановлює]  { $chance ->
                    [1] Спричиняє
                    *[other] спричиняє
                } {LOC($key)} принаймні на {NATURALFIXED($time, 3)} {MANY("секунду", $time)} без накопичення
        *[видаляє]{ $chance ->
                    [1] Видаляє
                    *[other] видаляє
                } {NATURALFIXED($time, 3)} {MANY("секунду", $time)} з {LOC($key)}
    } { $delay ->
        [0] негайно
        *[other] після {NATURALFIXED($delay, 3)} секунд затримки
    }

entity-effect-guidebook-status-effect-indef =
    { $type ->
        [update]{ $chance ->
                    [1] Спричиняє
                    *[other] спричиняє
                 } постійний {LOC($key)}
        [додає]   { $chance ->
                    [1] Спричиняє
                    *[other] спричиняє
                } постійний {LOC($key)}
        [встановлює]  { $chance ->
                    [1] Спричиняє
                    *[other] спричиняє
                } постійний {LOC($key)}
        *[видаляє]{ $chance ->
                    [1] Видаляє
                    *[other] видаляє
                } {LOC($key)}
    } { $delay ->
        [0] негайно
        *[other] після {NATURALFIXED($delay, 3)} секунд затримки
    }

entity-effect-guidebook-збивання з ніг =
    { $type ->
        [update]{ $chance ->
                    [1] Спричиняє
                    *[other] спричиняє
                    } {LOC($key)} принаймні на {NATURALFIXED($time, 3)} {MANY("секунду", $time)} без накопичення
        [додає]   { $chance ->
                    [1] Спричиняє
                    *[other] спричиняє
                } збивання з ніг принаймні на {NATURALFIXED($time, 3)} {MANY("секунду", $time)} з накопиченням
        *[встановлює]  { $chance ->
                    [1] Спричиняє
                    *[other] спричиняє
                } збивання з ніг принаймні на {NATURALFIXED($time, 3)} {MANY("секунду", $time)} без накопичення
        [видаляє]{ $chance ->
                    [1] Видаляє
                    *[other] видаляє
                } {NATURALFIXED($time, 3)} {MANY("секунду", $time)} з збивання з ніг
    }

entity-effect-guidebook-встановлює-solution-temperature-effect =
    { $chance ->
        [1] Встановлює
        *[other] встановлює
    } температуру розчину точно на {NATURALFIXED($temperature, 2)}k

entity-effect-guidebook-налаштовує-solution-temperature-effect =
    { $chance ->
        [1] { $deltasign ->
                [1] Додає
                *[-1] Видаляє
            }
        *[other]
            { $deltasign ->
                [1] додає
                *[-1] видаляє
            }
    } тепло з розчину поки не досягне { $deltasign ->
                [1] максимум {NATURALFIXED($maxtemp, 2)}k
                *[-1] мінімум {NATURALFIXED($mintemp, 2)}k
            }

entity-effect-guidebook-налаштовує-reagent-reagent =
    { $chance ->
        [1] { $deltasign ->
                [1] Додає
                *[-1] Видаляє
            }
        *[other]
            { $deltasign ->
                [1] додає
                *[-1] видаляє
            }
    } {NATURALFIXED($amount, 2)}u з {$reagent} { $deltasign ->
        [1] до
        *[-1] з
    } розчину

entity-effect-guidebook-налаштовує-reagent-group =
    { $chance ->
        [1] { $deltasign ->
                [1] Додає
                *[-1] Видаляє
            }
        *[other]
            { $deltasign ->
                [1] додає
                *[-1] видаляє
            }
    } {NATURALFIXED($amount, 2)}u з reagents in the group {$group} { $deltasign ->
            [1] до
            *[-1] з
        } розчину

entity-effect-guidebook-налаштовує-temperature =
    { $chance ->
        [1] { $deltasign ->
                [1] Додає
                *[-1] Видаляє
            }
        *[other]
            { $deltasign ->
                [1] додає
                *[-1] видаляє
            }
    } {POWERJOULES($amount)} з heat { $deltasign ->
            [1] до
            *[-1] з
        } тіла в якому знаходиться

entity-effect-guidebook-chem-спричиняє-disease =
    { $chance ->
        [1] Спричиняє
        *[other] спричиняє
    } хворобу { $disease }

entity-effect-guidebook-chem-спричиняє-random-disease =
    { $chance ->
        [1] Спричиняє
        *[other] спричиняє
    } хворобуs { $хвороби }

entity-effect-guidebook-тремтіння =
    { $chance ->
        [1] Спричиняє
        *[other] спричиняє
    } тремтіння

entity-effect-guidebook-clean-bloodstream =
    { $chance ->
        [1] Очищає
        *[other] очищає
    } the bloodstream з other chemicals

entity-effect-guidebook-лікує-disease =
    { $chance ->
        [1] Лікує
        *[other] лікує
    } хвороби

entity-effect-guidebook-eye-damage =
    { $chance ->
        [1] { $deltasign ->
                [1] Завдає
                *[-1] Лікує
            }
        *[other]
            { $deltasign ->
                [1] завдає
                *[-1] лікує
            }
    } пошкодження очей

entity-effect-guidebook-vomit =
    { $chance ->
        [1] Спричиняє
        *[other] спричиняє
    } блювоту

entity-effect-guidebook-створює-gas =
    { $chance ->
        [1] Створює
        *[other] створює
    } { $мольs } { $мольs ->
        [1] моль
        *[other] мольs
    } з { $gas }

entity-effect-guidebook-drunk =
    { $chance ->
        [1] Спричиняє
        *[other] спричиняє
    } сп'яніння

entity-effect-guidebook-вражає електрикою =
    { $chance ->
        [1] Вражає електрикою
        *[other] вражає електрикою
    } метаболізатора на {NATURALFIXED($time, 3)} {MANY("секунду", $time)}

entity-effect-guidebook-emote =
    { $chance ->
        [1] Will наce
        *[other] наce
    } метаболізатора до [bold][color=white]{$emote}[/color][/bold]

entity-effect-guidebook-гасить-reaction =
    { $chance ->
        [1] Гасить
        *[other] гасить
    } вогонь

entity-effect-guidebook-flammable-reaction =
    { $chance ->
        [1] Збільшує
        *[other] збільшує
    } займистість

entity-effect-guidebook-запалює =
    { $chance ->
        [1] Запалює
        *[other] запалює
    } метаболізатора

entity-effect-guidebook-робить-розумним =
    { $chance ->
        [1] Робить
        *[other] робить
    } метаболізатора розумним

entity-effect-guidebook-робить-перетворює =
    { $chance ->
        [1] Перетворює
        *[other] перетворює
    } метаболізатора inдо a { $entityname }

entity-effect-guidebook-змінює-bleed-amount =
    { $chance ->
        [1] { $deltasign ->
                [1] Викликає
                *[-1] Зменшує
            }
        *[other] { $deltasign ->
                    [1] викликає
                    *[-1] зменшує
                 }
    } кровотечу

entity-effect-guidebook-змінює-blood-level =
    { $chance ->
        [1] { $deltasign ->
                [1] Збільшує
                *[-1] Зменшує
            }
        *[other] { $deltasign ->
                    [1] збільшуєs
                    *[-1] зменшує
                 }
    } рівень крові

entity-effect-guidebook-паралізує =
    { $chance ->
        [1] Паралізує
        *[other] паралізує
    } метаболізатора принаймні на {NATURALFIXED($time, 3)} {MANY("секунду", $time)}

entity-effect-guidebook-movespeed-modifier =
    { $chance ->
        [1] Змінює
        *[other] змінює
    } швидкість руху на {NATURALFIXED($sprintspeed, 3)}x принаймні на {NATURALFIXED($time, 3)} {MANY("секунду", $time)}

entity-effect-guidebook-reвстановлює-narcolepsy =
    { $chance ->
        [1] Тимчасово відтерміновує
        *[other] тимчасово відтерміновує
    } зf narcolepsy

entity-effect-guidebook-змиває-cream-pie-reaction =
    { $chance ->
        [1] Змиває
        *[other] змиває
    } зf cream pie з one's face

entity-effect-guidebook-лікує-zombie-infection =
    { $chance ->
        [1] Лікує
        *[other] лікує
    } поточну зомбі-інфекцію

entity-effect-guidebook-спричиняє-zombie-infection =
    { $chance ->
        [1] Дає
        *[other] дає
    } особі зомбі-інфекцію

entity-effect-guidebook-innoculate-zombie-infection =
    { $chance ->
        [1] Лікує
        *[other] лікує
    } поточну зомбі-інфекцію, and provides immunity до future infections

entity-effect-guidebook-зменшує-rotting =
    { $chance ->
        [1] Регенерує
        *[other] регенерує
    } {NATURALFIXED($time, 3)} {MANY("секунду", $time)} з rotting

entity-effect-guidebook-area-reaction =
    { $chance ->
        [1] Спричиняє
        *[other] спричиняє
    } реакцію диму або піни на {NATURALFIXED($duration, 3)} {MANY("секунду", $duration)}

entity-effect-guidebook-додає-до-solution-reaction =
    { $chance ->
        [1] Спричиняє
        *[other] спричиняє
    } {$reagent} до be додаєed до its internal solution container

entity-effect-guidebook-artifact-unlock =
    { $chance ->
        [1] Допомагає
        *[other] допомагає
        } розблокувати інопланетний артефакт.

entity-effect-guidebook-artifact-durability-resдоre =
    Resдоres {$resдоred} міцність в активних вузлах інопланетного артефакту.

entity-effect-guidebook-plant-attribute =
    { $chance ->
        [1] Налаштовує
        *[other] налаштовує
    } {$attribute} by {$positive ->
    [true] [color=red]{$amount}[/color]
    *[false] [color=green]{$amount}[/color]
    }

entity-effect-guidebook-plant-cryoxadone =
    { $chance ->
        [1] Омолоджує
        *[other] омолоджує
    } the plant, depending on the plant's age and time до grow

entity-effect-guidebook-plant-phalanximine =
    { $chance ->
        [1] Відновлює
        *[other] відновлює
    } життєздатність рослини, зробленої нежиттєздатною мутацією

entity-effect-guidebook-plant-diethylamine =
    { $chance ->
        [1] Збільшує
        *[other] збільшує
    } тривалість життя рослини та/або базове здоров'я з 10% шансом на кожне

entity-effect-guidebook-plant-robust-harvest =
    { $chance ->
        [1] Збільшує
        *[other] збільшує
    } потужність рослини на {$збільшує} up до a maximum з {$limit}. Спричиняє the plant до lose its seeds once the potency reaches {$seedlesstreshold}. Trying до додає potency over {$limit} may спричиняє decrease in yield at a 10% chance

entity-effect-guidebook-plant-seeds-додає =
    { $chance ->
        [1] Resдоres the
        *[other] resдоre the
    } seeds з the plant

entity-effect-guidebook-plant-seeds-видаляє =
    { $chance ->
        [1] Видаляє the
        *[other] видаляє the
    } seeds з the plant

entity-effect-guidebook-plant-мутує-chemicals =
    { $chance ->
        [1] Мутує
        *[other] мутує
    } a plant до produce {$name}
