using Content.Server.Body.Components;
using Content.Server.Ghost.Roles.Events;
using Content.Server.Roles;
using Content.Server.Terminator.Components;
using Content.Shared.Roles;
using Robust.Shared.Map;

namespace Content.Server.Terminator.Systems;

/// <summary>
/// DeltaV - this is just used for paradox anomaly upstream doesnt use it anymore.
/// </summary>
public sealed class TerminatorSystem : EntitySystem
{
    public void SetTarget(Entity<TerminatorComponent?> ent, EntityUid mindId)
    {
        ent.Comp ??= EnsureComp<TerminatorComponent>(ent);
        ent.Comp.Target = mindId;
    }
}
