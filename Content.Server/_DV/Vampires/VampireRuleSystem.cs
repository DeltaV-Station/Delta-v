using Content.Server._DV.Vampires.Components;
using Content.Server.Antag;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Server.Roles;
using Content.Shared._DV.BloodDraining.Components;
using Content.Shared._DV.Vampires.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Vampires;

public sealed class VampireRuleSystem : GameRuleSystem<VampireRuleComponent>
{
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly RoleSystem _roleSystem = default!;

    private static readonly EntProtoId MindRole = "MindRoleVampire";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VampireRuleComponent, AfterAntagEntitySelectedEvent>(OnAntagSelect);
    }

    private void OnAntagSelect(Entity<VampireRuleComponent> uid, ref AfterAntagEntitySelectedEvent args)
    {
        if (!_mindSystem.TryGetMind(uid, out var mindId, out var mind))
            return;

        EnsureComp<VampireComponent>(uid);
        EnsureComp<BloodDrainerComponent>(uid);

        _roleSystem.MindAddRole(mindId, MindRole, mind, true);

        // TODO: Briefing and fluff
    }
}
