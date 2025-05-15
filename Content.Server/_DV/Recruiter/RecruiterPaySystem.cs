using Content.Server.Body.Components;
using Content.Server.Forensics;
using Content.Server.Objectives.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared._DV.Recruiter;
using Content.Shared.Popups;
using Content.Server.Mind;
using Content.Shared.Roles.Jobs;
using Content.Server.EntityEffects.Effects;

namespace Content.Server._DV.Recruiter;

/// <summary>
/// Handles Recruiter Payment
/// </summary>
public sealed class RecruiterPaySystem : EntitySystem
{

    [Dependency] private readonly SharedJobSystem _jobs = default!;
    private PayoutEvent user;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RecruiterPayComponent, PayoutEvent>(Payout);
    }

    private void Payout(Entity<RecruiterPayComponent> ent, ref PayoutEvent args)
    {
        return;
    }
}
