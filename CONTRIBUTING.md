# Delta-V Contributing Guidelines

Generally we follow [upstream's PR guidelines](https://docs.spacestation14.com/en/general-development/codebase-info/pull-request-guidelines.html) for code quality and such.

Upstream is the [space-wizards/space-station-14](https://github.com/space-wizards/space-station-14) repository that wizden runs on.

# Content specific to Delta-V

In general anything you create from scratch (not modifying something that exists from upstream) should go in a DeltaV subfolder.

Examples:
- `Content.Server/DeltaV/Chapel/SacrificialAltarSystem.cs`
- `Resources/Prototypes/DeltaV/ai_factions.yml`
- `Resources/Audio/DeltaV/Items/gavel.ogg`
- `Resources/Textures/DeltaV/Icons/cri.rsi`
- `Resources/Locale/en-US/deltav/shipyard/shipyard-console.ftl`
  The locale subfolder is lowercase `deltav` instead of `DeltaV`.
- `Resources/ServerInfo/Guidebook/DeltaV/AlertProcedure.xml`
  Note that guidebooks go in `ServerInfo/Guidebook/DeltaV` and not `ServerInfo/DeltaV`!

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

# Changelogs

By default any changelogs goes in the DeltaV changelog, you can use the DeltaV admin changelog by putting `DELTAVADMIN:` in a line after `:cl:`.

Do not use `ADMIN:` as **it will mangle** the upstream admin changelog!

# Additional resources

If you are new to contributing to SS14 in general, have a look at the [SS14 docs](https://docs.spacestation14.io/) or ask for help in `#contribution-help` on [Discord](https://go.delta-v.org/AtDxv)!
