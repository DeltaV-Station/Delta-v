using Content.Server._DV.Psionics.Systems;
using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Traits.Assorted;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Content.Shared.Roles.Components;

namespace Content.Server.GameTicking.Rules;

/// <summary>
/// Handles paradox anomaly related things when spawning paradox clones.
/// </summary>
public sealed partial class ParadoxCloneRuleSystem
{
    [Dependency] private readonly PsionicSystem _psionic = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;

    private void FilterTargets(HashSet<Entity<MindComponent>> minds)
    {
        // TODO: use generic IMindFilter
        // no picking other antags or non-crew and entities with no paradox clone trait
        minds.RemoveWhere(mind => _role.MindIsAntagonist(mind) ||
            !_role.MindHasRole<JobRoleComponent>((mind, mind), out var role) ||
            role?.Comp1.JobPrototype == null ||
            (mind.Comp.OwnedEntity is { } entity && HasComp<NoParadoxCloneComponent>(entity))
        );
    }

    private void PostClone(EntityUid mob)
    {
        // guaranteed psionic power
        var psi = EnsureComp<PotentialPsionicComponent>(mob);
        _psionic.AddRandomPsionicPower((mob, psi), false);
    }
}
