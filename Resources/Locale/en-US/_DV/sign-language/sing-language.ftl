# Sign Language Localization Strings

# ========================================
# Menu Category Labels
# ========================================

sign-menu-category-people = People
sign-menu-category-locations = Locations
sign-menu-category-objects = Objects
sign-menu-category-general = General

# ========================================
# Topic Labels - PEOPLE
# ========================================

sign-topic-me = I / ME
sign-topic-you = YOU
sign-topic-person = PERSON
sign-topic-crew = CREW
sign-topic-someone = SOMEONE

# ========================================
# Topic Labels - LOCATIONS
# ========================================

sign-topic-medbay = MEDBAY
sign-topic-bridge = BRIDGE
sign-topic-security = SECURITY
sign-topic-cargo = CARGO
sign-topic-eva = EVA
sign-topic-maint = MAINT
sign-topic-hallway = HALLWAY

# ========================================
# Topic Labels - OBJECTS
# ========================================

sign-topic-airlock = AIRLOCK
sign-topic-console = CONSOLE
sign-topic-apc = APC
sign-topic-machine = MACHINE

# ========================================
# Topic Labels - GENERAL
# ========================================

sign-topic-this = THIS
sign-topic-that = THAT
sign-topic-there = THERE
sign-topic-problem = PROBLEM
sign-topic-unknown = UNKNOWN

# ========================================
# Event Labels - PEOPLE
# ========================================

sign-event-injured = INJURED
sign-event-crit = CRIT
sign-event-dead = DEAD
sign-event-trapped = TRAPPED
sign-event-missing = MISSING
sign-event-bleeding = BLEEDING

# ========================================
# Event Labels - LOCATIONS
# ========================================

sign-event-fire = FIRE
sign-event-damaged = DAMAGED
sign-event-flooded = FLOODED
sign-event-dangerous = DANGEROUS
sign-event-secure = SECURE

# ========================================
# Event Labels - GENERAL
# ========================================

sign-event-safe = SAFE
sign-event-not-safe = NOT SAFE
sign-event-confused = CONFUSED
sign-event-problem = PROBLEM

# ========================================
# Event Labels - OBJECTS
# ========================================

sign-event-broken = BROKEN
sign-event-malfunctioning = MALFUNCTIONING

# ========================================
# Intent Labels
# ========================================

sign-intent-fix = FIX
sign-intent-now = NOW
sign-intent-evacuate = EVACUATE
sign-intent-follow-me = FOLLOW ME
sign-intent-need-help = NEED HELP
sign-intent-wait = WAIT
sign-intent-stop = STOP
sign-intent-avoid = AVOID

# ========================================
# Intensity Labels
# ========================================

sign-intensity-neutral = Neutral
sign-intensity-urgent = Urgent
sign-intensity-calm = Calm
sign-intensity-panic = Panic
sign-intensity-repeat = Repeat

# ========================================
# Sign Output Messages
# ========================================

# For non-fluent observers
sign-language-nonfluent-message = makes {$description} hand signs toward {$topic}.
sign-intensity-urgent-nonfluent = sharp, urgent
sign-intensity-calm-nonfluent = slow, deliberate
sign-intensity-panic-nonfluent = frantic, panicked
sign-intensity-repeat-nonfluent = repetitive, emphatic
sign-intensity-neutral-nonfluent = measured
sign-language-nonfluent-default-description = deliberate

# Fluent wrapper - action in italics, signed content normal
# Uses select expression on intensity ID to pick the adverb
sign-language-wrap-fluent = [italic]{ PROPER($entity) ->
    *[false] the {$entityName} signs{ $intensity ->
        [SignIntensityUrgent] { " urgently" }
        [SignIntensityCalm] { " calmly" }
        [SignIntensityPanic] { " frantically" }
        [SignIntensityRepeat] { " repeatedly" }
        *[other] { "" }
    }:[/italic] "{$topic} — {$event} — {$intent}{$formatting}"
     [true] {CAPITALIZE($entityName)} signs{ $intensity ->
        [SignIntensityUrgent] { " urgently" }
        [SignIntensityCalm] { " calmly" }
        [SignIntensityPanic] { " frantically" }
        [SignIntensityRepeat] { " repeatedly" }
        *[other] { "" }
    }:[/italic] "{$topic} — {$event} — {$intent}{$formatting}"
    }

# Fluent wrapper with bold signed content (for panic, urgent emphasis)
sign-language-wrap-fluent-bold = [italic]{ PROPER($entity) ->
    *[false] the {$entityName} signs{ $intensity ->
        [SignIntensityUrgent] { " urgently" }
        [SignIntensityCalm] { " calmly" }
        [SignIntensityPanic] { " frantically" }
        [SignIntensityRepeat] { " repeatedly" }
        *[other] { "" }
    }:[/italic] [bold]"{$topic} — {$event} — {$intent}{$formatting}"[/bold]
     [true] {CAPITALIZE($entityName)} signs{ $intensity ->
        [SignIntensityUrgent] { " urgently" }
        [SignIntensityCalm] { " calmly" }
        [SignIntensityPanic] { " frantically" }
        [SignIntensityRepeat] { " repeatedly" }
        *[other] { "" }
    }:[/italic] [bold]"{$topic} — {$event} — {$intent}{$formatting}"[/bold]
    }

# Non-fluent wrapper - entire message in italics
sign-language-wrap-nonfluent = [italic]{ PROPER($entity) ->
    *[false] the {$entityName} makes {$description} hand signs toward {$topic}.[/italic]
     [true] {CAPITALIZE($entityName)} makes {$description} hand signs toward {$topic}.[/italic]
    }

# Raw fluent message (used for chat log, accessibility)
sign-language-fluent-message = signs{ $intensity ->
        [SignIntensityUrgent] { " urgently" }
        [SignIntensityCalm] { " calmly" }
        [SignIntensityPanic] { " frantically" }
        [SignIntensityRepeat] { " repeatedly" }
        *[other] { "" }
    }: "{$topic} — {$event} — {$intent}{$formatting}"


# ========================================
# Preview Overlay Helper Messages
# ========================================

sign-preview-select-topic = Select a topic...
sign-preview-select-event = Select what's happening...
sign-preview-select-intent = Select an intent...
sign-preview-complete = Press ENTER to send, ESC to cancel
sign-language-need-free-hand = You need at least one free hand to sign.
