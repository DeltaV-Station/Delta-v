markings-search = Search
-markings-selection = { $selectable ->
    [0] You have no markings remaining.
    [one] You can select one more marking.
   *[other] You can select { $selectable } more markings.
}
markings-limits = { $required ->
    [true] { $count ->
        [-1] Select at least one marking.
        [0] You cannot select any markings, but somehow, you have to? This is a bug.
        [one] Select one marking.
       *[other] Select at least one marking and up to {$count} markings. { -markings-selection(selectable: $selectable) }
    }
   *[false] { $count ->
        [-1] Select any number of markings.
        [0] You cannot select any markings.
        [one] Select up to one marking.
       *[other] Select up to {$count} markings. { -markings-selection(selectable: $selectable) }
    }
}
markings-reorder = Reorder markings

humanoid-marking-modifier-respect-limits = Respect limits
humanoid-marking-modifier-respect-group-sex = Respect group & sex restrictions
humanoid-marking-modifier-base-layers = Base layers
humanoid-marking-modifier-enable = Enable
humanoid-marking-modifier-prototype-id = Prototype id:

humanoid-marking-modifier-force = Force
humanoid-marking-modifier-ignore-species = Ignore Species
humanoid-marking-modifier-base-layers = Base layers
humanoid-marking-modifier-enable = Enable
humanoid-marking-modifier-prototype-id = Prototype id:

# Categories

markings-category-Special = Special
markings-category-Hair = Hair
markings-category-FacialHair = Facial Hair
markings-category-Head = Head
markings-category-HeadTop = Head (Top)
markings-category-HeadSide = Head (Side)
markings-category-Snout = Snout
markings-category-SnoutCover = Snout (Cover)
markings-category-UndergarmentTop = Undergarment (Top)
markings-category-UndergarmentBottom = Undergarment (Bottom)
markings-category-Chest = Chest
markings-category-Arms = Arms
markings-category-Legs = Legs
markings-category-Tail = Tail
markings-category-Overlay = Overlay
markings-category-RArmExtension = Wings
