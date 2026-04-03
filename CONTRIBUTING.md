# NovaSector Contributing Guidelines

Generally we follow [upstream's PR guidelines](https://docs.spacestation14.com/en/general-development/codebase-info/pull-request-guidelines.html) for code quality and such.

Importantly do not make webedits, copied verbatim from above:
> Do not use GitHub's web editor to create PRs. PRs submitted through the web editor may be closed without review.

Upstream is the [DeltaV-Station/Delta-v](https://github.com/DeltaV-Station/Delta-v) repository.

# Content specific to NovaSector

In general anything you create from scratch (not modifying something that exists from upstream) should go in a NovaSector subfolder, `_Nova`.

Examples:
- `Content.Server/_Nova/Species/MySpeciesSystem.cs`
- `Resources/Prototypes/_Nova/Species/myspecies.yml`
- `Resources/Audio/_Nova/Emotes/howl.ogg`
- `Resources/Textures/_Nova/Mobs/Customization/tails.rsi`
- `Resources/Locale/en-US/_Nova/species/myspecies.ftl`
- `Resources/ServerInfo/Guidebook/_Nova/Rules/NovaSectorRuleset.xml`
  Note that guidebooks go in `ServerInfo/Guidebook/_Nova` and not `ServerInfo/_Nova`!

# Changes to upstream files

Follow a few guidelines when modifying non-NovaSector files, to help us manage our project. (files that are not in `_Nova`, `_DV` or `Nyano` folders)

Primarily, **add comments on or around all new or changed lines** in upstream files. Explain what was changed to make resolving merge conflicts easier; we regularly merge new upstream changes into our project.

### Changing Upstream YAML .yml files

**Add comments on or around any changed lines.**

If you add a new component to a prototype, add an explanation to the `type: ...` line. Example:

```yml
- type: entity
  parent: MobSiliconBase
  id: MobSupplyBot
  components:
  - type: InteractionPopup # Nova - Make supplybots pettable
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
    - id: ClothingShoesBootsSecurityMagboots # Nova - Added security magboots.
    - id: ClothingShoesBootsJack
    #- id: ClothingOuterCoatWarden # Nova - removed for incongruence
    # Begin Nova additions
    - id: WeaponEnergyShotgun
    - id: LunchboxSecurityFilledRandom
      prob: 0.3
    # End Nova additions
```

### Changing Upstream C# .cs files

If you are adding a lot of C# code, then take advantage of partial classes. Put the new code in its own file in the `_Nova` folder, if it makes sense.

Otherwise, **add comments on or around any changed lines.**

A comment on a new imported namespace:
```cs
using Content.Server._Nova.MySystem; // Nova
```

A pair of comments enclosing a block of added code:
```cs
private EntityUid Slice(...)
{
    ...

    _transform.SetLocalRotation(sliceUid, 0);

    // Nova - start of custom code
    var slicedEv = new FoodSlicedEvent(user, uid, sliceUid);
    RaiseLocalEvent(uid, ref slicedEv);
    // Nova - end of custom code

    ...
}
```

### Changing Upstream Localization Fluent .ftl files

**Move all changed locale strings to a new Nova file** - use a `.ftl` file in the `_Nova` folder. Comment out the old strings in the upstream file, and explain that they were moved.

### Early merges

We mostly merge upstream changes in big chunks (e.g. a month of upstream PRs at a time), but urgent changes can be merged early, separately.

Early merges are an exception to the above rules - if cherry-picking a PR for an early merge, you don't need to add `# Nova` comments, since the code is coming directly from upstream without any changes.

# Mapping

If you want to make changes to a map, get in touch with its maintainer to make sure you don't both make changes at the same time.

Conflicts with maps make PRs mutually exclusive so either your work or the maintainer's work will be lost, communicate to avoid this!

Please make a detailed list of **all** changes (even minor changes) with locations when submitting a PR.

**Submitting a map PR**

Please limit changelogs on map PRs to **significant** map alterations or additions. Minor map edits do not need changelogs.
Format for map PRs looks like:
```
:cl: Yourname
MAPS:
- add: Mapname: Added fun!
- remove: Mapname: Removed fun!
- tweak: Mapname: Changed fun!
- fix: Mapname: Fixed fun!
```

# Before you submit

Double-check your diff on GitHub before submitting: look for unintended commits or changes and remove accidental whitespace or line-ending changes.

# Changelogs

By default any changelogs go in the NovaSector changelog.

# Additional resources

If you are new to contributing to SS14 in general, have a look at the [SS14 docs](https://docs.spacestation14.io/)!
