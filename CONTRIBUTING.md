# All-Access Contributing Guidelines

Speak directly with Lilith or Rox on this, right? This document

# Content specific to Delta-V

In general anything you create from scratch (not modifying something that exists from upstream) should go in a All Access subfolder, `_AA`.

Examples:
- `Content.Server/_AA/wedonthaveanyexamplesyetlol`
  Note that guidebooks go in `ServerInfo/Guidebook/_AA` and not `ServerInfo/_AA`!

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
  id: SecurityPDA
  name: HR PDA # All Access - HR Department replacing Security
```

A pair of comments enclosing a list of added items to starting gear (GET BETTER EXAMPLE LATER PLEASE):
```yml
  storage:
    back:
    - OldItem
    # Begin All Access additions
    - NewItem1
    - NewItem2
    # End All Access additions
```

A comment on a new imported namespace:
```cs
using Content.Server.Tension.Liability; // All Access
```

A pair of comments enclosing a block of added code (THIS IS A DELTA V EXAMPLE. I DID NOT COME UP WITH AN ALL ACCESS EXAMPLE YET SINCE WE DON'T HAVE ADDITIONS YET. PLZ):
```cs
private EntityUid Slice(...)
{
    ...

    _transform.SetLocalRotation(sliceUid, 0);

    // All Acces - start of deep frier stuff
    var slicedEv = new FoodSlicedEvent(user, uid, sliceUid);
    RaiseLocalEvent(uid, ref slicedEv);
    // All Access - end of deep frier stuff

    ...
}
```

# Mapping

If you want to make changes to a map, get in touch with its maintainer to make sure you don't both make changes at the same time.

Conflicts with maps make PRs mutually exclusive so either your work or the maintainer's work will be lost, communicate to avoid this!

Please make a detailed list of **all** changes(even minor changes) with locations when submitting a PR. This helps reviewers hone in on them without having to search an entire map for differences. Ex: [Map Edits](https://github.com/AllAccess-Station/All-Access/pull/8675)


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

Trying to PR AI-generated content may result in you being mocked relentlessly. Like seriously? Give it a minute and try making something yourself next time.
