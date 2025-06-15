using Content.Server._DV.Vampires.Components;
using Content.Server.Actions;
using Content.Server.Antag;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Server.Roles;
using Content.Shared._DV.BloodDraining.Components;
using Content.Shared._DV.Vampires.Components;
using Robust.Server.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Vampires;

public sealed class VampireRuleSystem : GameRuleSystem<VampireRuleComponent>
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly RoleSystem _roleSystem = default!;

    private static readonly EntProtoId MindRole = "MindRoleVampire";
    private readonly EntProtoId _mistFormAction = "ActionMistForm";
    private readonly EntProtoId _hypnoticAction = "ActionHypnoticGaze";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VampireRuleComponent, AfterAntagEntitySelectedEvent>(OnAntagSelect);
    }

    private void OnAntagSelect(Entity<VampireRuleComponent> uid, ref AfterAntagEntitySelectedEvent args)
    {
        var progenitor = args.EntityUid;

        if (!_mindSystem.TryGetMind(progenitor, out var mindId, out var mind))
            return;

        EnsureComp<VampireComponent>(progenitor, out var progenitorComp);
        EnsureComp<BloodDrainerComponent>(progenitor);

        _roleSystem.MindAddRole(mindId, MindRole, mind, true);

        AddActions((progenitor, progenitorComp), true);

        // TODO: Briefing and fluff
    }

    public void ConvertToLesserSpawn(Entity<VampireComponent> progenitor, EntityUid victim)
    {
        if (!_mindSystem.TryGetMind(victim, out var mindId, out var mind) ||
            !_playerManager.TryGetSessionById(mind.UserId, out var session))
            return;

        EnsureComp<VampireComponent>(victim, out var spawnComp);
        EnsureComp<BloodDrainerComponent>(victim);

        _roleSystem.MindAddRole(mindId, MindRole, mind, true);

        AddActions((victim, spawnComp), false);

        // TODO: Briefing and fluff
    }

    private void AddActions(Entity<VampireComponent> ent, bool isProgenitor)
    {
        _actions.AddAction(ent, _mistFormAction);
        if (isProgenitor)
        {
            // Only progenitors get the hypnotic gaze ability
            _actions.AddAction(ent, _hypnoticAction);
        }
    }
}
