bounty-console-menu-title = Cargo bounty console
bounty-console-label-button-text = Print label
bounty-console-skip-button-text = Skip
bounty-console-time-label = Time: [color=orange]{$time}[/color]
bounty-console-reward-label = Reward: [color=limegreen]${$reward}[/color]
bounty-console-manifest-label = Manifest: [color=orange]{$item}[/color]
bounty-console-manifest-entry =
    { $amount ->
        [1] {$item}
        *[other] {$item} x{$amount}
    }
bounty-console-manifest-reward = Reward: ${$reward}
bounty-console-description-label = [color=gray]{$description}[/color]
bounty-console-id-label = ID#{$id}

## Begin DeltaV
bounty-console-claim-button-text = Claim
bounty-console-claimed-by-none = None
bounty-console-claimed-by-unknown = Unknown
bounty-console-claimed-by = Claimed by: {$claimant}

bounty-console-status-Undelivered = Undelivered
bounty-console-status-Waiting = Processing
bounty-console-status-OnShuttle = On Shuttle

bounty-console-status-formatted-Undelivered = [color=orange]Undelivered[/color]
bounty-console-status-formatted-Waiting = Processing
bounty-console-status-formatted-OnShuttle = [color=limegreen]On Shuttle[/color]

bounty-console-status-tooltip-Undelivered = This bounty has not yet been sent out for fulfilment
bounty-console-status-tooltip-Waiting = This bounty has been sent out, and is waiting to be fulfilled
bounty-console-status-tooltip-OnShuttle = This bounty is completed, ready to be delivered to the trade station
## End DeltaV

bounty-console-flavor-left = Bounties sourced from local unscrupulous dealers.
bounty-console-flavor-right = v1.4

bounty-manifest-header = [font size=14][bold]Official cargo bounty manifest[/bold] (ID#{$id})[/font]
bounty-manifest-list-start = Item manifest:

bounty-console-tab-available-label = Available
bounty-console-tab-history-label = History
bounty-console-history-empty-label = No bounty history found
bounty-console-history-notice-completed-label = [color=limegreen]Completed[/color]
bounty-console-history-notice-skipped-label = [color=red]Skipped[/color] by {$id}
