discord-watchlist-connection-header = { $players ->
        [one] {$players} гравець у списку спостереження під'єднався
        *[other] {$players} гравців у списку спостереження під'єдналися
    } до {$serverName}

discord-watchlist-connection-entry = - {$playerName} з повідомленням "{$message}"{ $expiry ->
        [0] {""}
        *[other] {" "}(закінчується <t:{$expiry}:R>)
    }{ $otherWatchlists ->
        [0] {""}
        [one] {" "}і ще {$otherWatchlists} інший список спостереження
        *[other] {" "}і ще {$otherWatchlists} інших списків спостереження
    }
