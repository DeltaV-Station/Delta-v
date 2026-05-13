### Interaction Messages

# System

## When trying to ingest without the required utensil... but you gotta hold it
ingestion-you-need-to-hold-utensil = Вам потрібно тримати в руках {INDEFINITE($utensil)} {$utensil}, щоб це з’їсти!

ingestion-try-use-is-empty = { CAPITALIZE(THE($object))} порожній!
ingestion-try-use-wrong-utensil = Ви не можете {$verb}{THE($food)} з {INDEFINITE($utensil)} {$utensil}.

ingestion-remove-mask = Спочатку потрібно зняти {$entity}.

## Failed Ingestion

ingestion-you-cannot-ingest-any-more = Ви більше не можете {$verb}!
ingestion-other-cannot-ingest-any-more = {CAPITALIZE(SUBJECT($target))} більше не може {$verb}!

ingestion-cant-digest = Ви не можете переварити {THE($entity)}!
ingestion-cant-digest-other = {CAPITALIZE(SUBJECT($target))} не може переварити {THE($entity)}!

## Action Verbs, not to be confused with Verbs

ingestion-verb-food = Їсти
ingestion-verb-drink = пити

# Edible Component

edible-nom = Ном. {$flavors}
edible-nom-other = Ном.
edible-slurp = Хльокати. {$flavors}
edible-slurp-other = Хльокати.
edible-swallow = Ви ковтаєте {THE($food)}
edible-gulp = ковток. {$flavors}
edible-gulp-other = ковток.

edible-has-used-storage = Ви не можете {$verb}{ THE($food) } з предметом, що зберігається всередині.

## Nouns

edible-noun-edible = їстівний
edible-noun-food = харчування
edible-noun-drink = пити
edible-noun-pill = таблетка

## Verbs

edible-verb-edible = ковтати
edible-verb-food = їсти
edible-verb-drink = пити
edible-verb-pill = ковтати

## Force feeding

edible-force-feed = {CAPITALIZE(THE($user))} намагається зробити вам {$verb}щось!
edible-force-feed-success = {CAPITALIZE(THE($user))} змусив вас {$verb}щось! {$flavors}
edible-force-feed-success-user = Ви успішно нагодували {THE($target)}