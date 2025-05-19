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

If you make a change to an upstream C# or YAML file **you must add comments on or around the changed lines**.
The comments should clarify what changed, to make conflict resolution simpler when a file is changed upstream.

For YAML specifically, if you add a new component to a prototype add the comment to the `type: ...` line.
If you just modify some fields of a component, comment the fields instead.

For C# files, if you are adding a lot of code try to put it in a partial class when it makes sense.

The exception to this is early merging commits that are going to be cherry picked in the future regardless, there's no harm in leaving them as-is.

As an aside, fluent (.ftl) files **do not support comments on the same line** as a locale value, so be careful when changing them.

## Examples of comments in upstream files

A single line comment on a changed yml field:
```yml
- type: entity
  parent: BasePDA
  id: SciencePDA
  name: epistemics PDA # DeltaV - Epistemics Department replacing Science
```

A pair of comments enclosing a list of added items to starting gear:
```yml
  storage:
    back:
    - EmergencyRollerBedSpawnFolded
    # Begin DeltaV additions
    - BodyBagFolded
    - Portafib
    # End DeltaV additions
```

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

# Mapping

If you want to make changes to a map, get in touch with its maintainer to make sure you don't both make changes at the same time.

Conflicts with maps make PRs mutually exclusive so either your work or the maintainer's work will be lost, communicate to avoid this!

Please make a detailed list of **all** changes(even minor changes) with locations when submitting a PR. This helps reviewers hone in on them without having to search an entire map for differences. Ex: [Map Edits](https://github.com/DeltaV-Station/Delta-v/pull/3165)

Please limit changelogs on map PRs to **significant** map alterations or additions. Minor map edits do not need changelogs. Limit changelogs to one per PR.

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
