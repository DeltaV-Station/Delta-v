leash-attaching-popup-self = You are trying to attach a leash to {$selfAnchor ->
    [false] {THE($target)}'s {$anchor}
    *[true] {THE($target)}
}...
leash-attaching-popup-target = {THE($target)} is trying to attach a leash to {$selfAnchor ->
    [false] your {$anchor}
    *[true] you
}...
leash-attaching-popup-others = {THE($user)} is trying to attach a leash to {$selfAnchor ->
    [false] {THE($target)}'s {$anchor}
    *[true] {THE($target)}
}

leash-detaching-popup-self = You are trying to remove the leash...
leash-detaching-popup-others = {THE($user)} is trying to remove the leash {$isSelf ->
    [true] from {REFLEXIVE($user)}
    *[false] from {THE($target)}
}...

leash-snap-popup = {THE($leash)} snaps off!
leash-set-length-popup = Length set to {$length}m.

leash-length-examine-text = Its current length is {$length}m.
