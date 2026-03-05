# Mail-related commands

## mailto
cmd-mailto-hint-recipient = Recipient EntityUid (must have MailReceiverComponent)
cmd-mailto-hint-container = Container EntityUid (entity with contents to transfer)
cmd-mailto-hint-fragile = Is fragile? (true/false)
cmd-mailto-hint-priority =  Is priority? (true/false)
cmd-mailto-hint-large = Is large? (true/false, optional)
cmd-mailto-description = Queue a parcel to be delivered to an entity. The target container's contents will be transferred to an actual mail parcel.
cmd-mailto-help = Usage: {$command} <recipient entityUid> <container entityUid> [is-fragile: true or false] [is-priority: true or false] [is-large: true or false, optional]
cmd-mailto-no-mailreceiver = Target recipient entity does not have a {$requiredComponent}.
cmd-mailto-no-blankmail = The {$blankMail} prototype doesn't exist. Something is very wrong. Contact a programmer.
cmd-mailto-bogus-mail = {$blankMail} did not have {$requiredMailComponent}. Something is very wrong. Contact a programmer.
cmd-mailto-invalid-container = Target container entity does not have a {$requiredContainer} container.
cmd-mailto-unable-to-receive = Target recipient entity was unable to be setup for receiving mail. ID may be missing.
cmd-mailto-no-teleporter-found = Target recipient entity was unable to be matched to any station's mail teleporter. Recipient may be off-station.
cmd-mailto-success = Success! Mail parcel has been queued for next teleport in {$timeToTeleport} seconds.

## mailnow
cmd-mailnow = Force all mail teleporters to deliver another round of mail as soon as possible. This will not bypass the undelivered mail limit.
cmd-mailnow-help = Usage: {$command}
cmd-mailnow-success = Success! All mail teleporters will be delivering another round of mail soon.
