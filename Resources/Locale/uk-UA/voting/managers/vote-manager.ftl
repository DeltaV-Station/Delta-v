# Displayed as initiator of vote when no user creates the vote
ui-vote-initiator-server = Сервер

## Default.Votes

ui-vote-restart-title = Рестарт
ui-vote-restart-succeeded = Голосування за перезапуск успішне.
ui-vote-restart-failed = Голосування за перезапуск провалилось (потрібно { TOSTRING($ratio, "P0") }).
ui-vote-restart-fail-not-enough-ghost-players = Помилка голосування за перезапуск: для ініціювання голосування за перезапуск потрібна мінімум { $ghostPlayerRequirement }% гравців-привидів. Наразі не вистачає гравців-привидів.
ui-vote-restart-yes = Так
ui-vote-restart-no = Ні
ui-vote-restart-abstain = Утриматись

ui-vote-gamemode-title = Наступний ігровий режим
ui-vote-gamemode-tie = Нічия у голосуванні! Вибираємо... { $picked }
ui-vote-gamemode-win = { $winner } виграв голосування режиму гри!

ui-vote-map-title = Наступна мапа
ui-vote-map-tie = Нічия у голосувані! Вибираємо... { $picked }
ui-vote-map-win = { $winner } виграв голосування за мапу!
ui-vote-map-notlobby = Голосування за мапу дійсне лише в лобі!
ui-vote-map-notlobby-time = Голосування за мапи дійсне лише в передраундовому лобі з { $time } залишилось!


# Votekick votes
ui-vote-votekick-unknown-initiator = Гравець
ui-vote-votekick-unknown-target = Невідомий гравець
ui-vote-votekick-title = { $initiator } викликав votekick для користувача: { $targetEntity }. Причина: { $reason }
ui-vote-votekick-yes = так
ui-vote-votekick-no = немає
ui-vote-votekick-abstain = Утриматись
ui-vote-votekick-success = Голосування за { $target } виконано успішно. Причина голосування: { $reason }
ui-vote-votekick-failure = Votekick для { $target } не вдалося. Причина голосування: { $reason }
ui-vote-votekick-not-enough-eligible = Недостатньо онлайн-учасників, які мають право голосувати, щоб розпочати голосування: { $voters }/{ $requirement }
ui-vote-votekick-server-cancelled = Votekick для { $target } було скасовано сервером.