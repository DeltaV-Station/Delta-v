# Delta-V Contributing Guidelines

Generally we follow [upstream's PR guidelines](https://docs.spacestation14.com/en/general-development/codebase-info/pull-request-guidelines.html) for code quality and such.

Importantly do not make webedits, copied verbatim from above:
> Do not use GitHub's web editor to create PRs. PRs submitted through the web editor may be closed without review.

Upstream is the [space-wizards/space-station-14](https://github.com/space-wizards/space-station-14) repository that wizden runs on.

# Content specific to Delta-V

In general anything you create from scratch (not modifying something that exists from upstream) should go in a DeltaV subfolder, `_DV`.

Examples:
- `Content.Server/_DV/Chapel/SacrificialAltarSystem.cs`
- `Resources/Prototypes/_DV/ai_factions.yml`
- `Resources/Audio/_DV/Items/gavel.ogg`
- `Resources/Textures/_DV/Icons/cri.rsi`
- `Resources/Locale/en-US/_DV/shipyard/shipyard-console.ftl`
- `Resources/ServerInfo/Guidebook/_DV/AlertProcedure.xml`
  Note that guidebooks go in `ServerInfo/Guidebook/_DV` and not `ServerInfo/_DV`!

# Changes to upstream files

Follow a few guidelines when modifying non-DeltaV files, to help us manage our project. (files that are not in `_DV` or `Nyano` folders)

Primarily, **add comments on or around all new or changed lines** in upstream files. Explain what was changed to make resolving merge conflicts easier; we regularly merge new upstream changes into our project.

### Changing Upstream YAML .yml files

**Add comments on or around any changed lines.**

If you add a new component to a prototype, add an explanation to the `type: ...` line. Example:

```yml
- type: entity
  parent: MobSiliconBase
  id: MobSupplyBot
  components:
  - type: InteractionPopup # DeltaV - Make supplybots pettable
    interactSuccessString: petting-success-supplybot
    interactFailureString: petting-failure-supplybot
    interactSuccessSound:
      path: /Audio/Ambience/Objects/periodic_beep.ogg
```

Whereas if you just modify some fields of a component, comment the fields instead, using inline or block comments. Examples:

```yml
- type: entityTable
  id: FillLockerWarden
  table: !type:AllSelector
    children:
    - id: ClothingHandsGlovesCombat
    - id: ClothingShoesBootsSecurityMagboots # DeltaV - Added security magboots.
    - id: ClothingShoesBootsJack
    #- id: ClothingOuterCoatWarden # DeltaV - removed for incongruence
    #- id: ClothingOuterWinterWarden # DeltaV - removed for incongruence
    - id: RubberStampWarden
    - id: DoorRemoteArmory
    - id: HoloprojectorSecurity
    # Begin DeltaV additions
    - id: WeaponEnergyShotgun
    - id: BoxPDAPrisoner
    - id: LunchboxSecurityFilledRandom
      prob: 0.3
    # End DeltaV additions
```

### Changing Upstream C# .cs files

If you are adding a lot of C# code, then take advantage of partial classes. Put the new code in its own file in the `_DV` folder, if it makes sense.

Otherwise, **add comments on or around any changed lines.**

A comment on a new imported namespace:
```cs
using Content.Server.Psionics.Glimmer; // DeltaV
```

A pair of comments enclosing a block of added code:
```cs
private EntityUid Slice(...)
{
    ...

    _transform.SetLocalRotation(sliceUid, 0);

    // DeltaV - start of deep frier stuff
    var slicedEv = new FoodSlicedEvent(user, uid, sliceUid);
    RaiseLocalEvent(uid, ref slicedEv);
    // DeltaV - end of deep frier stuff

    ...
}
```

### Changing Upstream Localization Fluent .ftl files

**Move all changed locale strings to a new DeltaV file** - use a `.ftl` file in the `_DV` folder. Comment out the old strings in the upstream file, and explain that they were moved.

Example:

Commented out old string in `Resources\Locale\en-US\xenoarchaeology\artifact-analyzer.ftl`
```
# DeltaV - moved to _DV file
#analysis-console-info-effect-value = [font="Monospace" size=11][color=gray]{ $state ->
#    [true] {$info}
#    *[false] Unlock nodes to gain info
#}[/color][/font]
```

The new version of the string in `Resources\Locale\en-US\_DV\xenoarchaeology\artifact-analyzer.ftl`
```
analysis-console-info-effect-value = [font="Monospace" size=11][color=gray]{ $state ->
    [vagueandspecific] {$vagueInfo} ({$specificInfo})
    [vagueonly] {$vagueInfo} (unable to detect details)
    [simple] {$specificInfo}
    [hidden] Unable to detect (unlock to discover)
    *[noinfo] Unlock nodes to gain info
}[/color][/font]
```

Also keep in mind that fluent (.ftl) files **do not support comments on the same line** as a locale value, so be careful when commenting.

### Early merges

We mostly merge upstream changes in big chunks (e.g. a month of upstream PRs at a time), but urgent changes can be merged early, separately.

Early merges are an exception to the above rules - if cherry-picking a PR for an early merge, you don't need to add `#DeltaV` comments, since the code is coming directly from upstream without any changes.

# Mapping

If you want to make changes to a map, get in touch with its maintainer to make sure you don't both make changes at the same time.

Conflicts with maps make PRs mutually exclusive so either your work or the maintainer's work will be lost, communicate to avoid this!

Please make a detailed list of **all** changes(even minor changes) with locations when submitting a PR. This helps reviewers hone in on them without having to search an entire map for differences. Ex: [Map Edits](https://github.com/DeltaV-Station/Delta-v/pull/3165)


**Submitting a map PR**

Please limit changelogs on map PRs to **significant** map alterations or additions. Minor map edits do not need changelogs.
Format for map PRs looks like:
```
:cl: Yourname
MAPS: Mapname
- add: Added fun!
- remove: Removed fun!
- tweak: Changed fun!
- fix: Fixed fun!
``` 

# Before you submit

Double-check your diff on GitHub before submitting: look for unintended commits or changes and remove accidental whitespace or line-ending changes.

Additionally for long-lasting PRs, if you see `RobustToolbox` in the changed files you have to revert it, use `git checkout upstream/master RobustToolbox` (replacing `upstream` with the name of your DeltaV-Station/Delta-V remote)

# Changelogs

By default any changelogs goes in the DeltaV changelog, you can use the DeltaV admin changelog by putting `DELTAVADMIN:` in a line after `:cl:`.

Do not use `ADMIN:` as **it will mangle** the upstream admin changelog!

# Additional resources

If you are new to contributing to SS14 in general, have a look at the [SS14 docs](https://docs.spacestation14.io/) or ask for help in `#contribution-help` on [Discord](https://discord.gg/deltav)!

## AI-Generated Content
Code, sprites and any other AI-generated content is not allowed to be submitted to the repository.

Trying to PR AI-generated content may result in you being banned from contributing.
