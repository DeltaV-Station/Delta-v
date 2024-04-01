using Content.Server.Chat.Managers;
using Content.Server.DeltaV.SpaceFerret.Components;
using Content.Server.GenericAntag;
using Content.Server.Roles;
using Content.Shared.DeltaV.SpaceFerret;
using Content.Shared.Interaction.Events;
using Content.Shared.Mind;
using Content.Shared.Nutrition;
using Robust.Shared.Audio;

namespace Content.Server.DeltaV.SpaceFerret.Systems;

public sealed class SpaceFerretSystem : EntitySystem
{
    [Dependency] private readonly RoleSystem _role = default!;
    [Dependency] private readonly IChatManager _chatMan = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpaceFerretComponent, GenericAntagCreatedEvent>(OnInit);
        SubscribeLocalEvent<SpaceFerretComponent, InteractionAttemptFailed>(OnInteractFailed);
        SubscribeLocalEvent<SpaceFerretComponent, HungerModifiedEvent>(OnHungerModified);
    }

    private void OnInit(EntityUid uid, SpaceFerretComponent component, GenericAntagCreatedEvent args)
    {
        var mind = args.Mind;

        if (mind.Session == null)
            return;

        var session = mind.Session;
        _role.MindAddRole(args.MindId, new RoleBriefingComponent
        {
            Briefing = Loc.GetString(component.RoleBriefing)
        }, mind);
        _role.MindAddRole(args.MindId, new SpaceFerretRoleComponent()
        {
            PrototypeId = component.AntagProtoId
        }, mind);
        _role.MindPlaySound(args.MindId, new SoundPathSpecifier(component.RoleIntroSfx), mind);
        _chatMan.DispatchServerMessage(session, Loc.GetString(component.RoleGreeting));
    }

    public void OnInteractFailed(EntityUid uid, SpaceFerretComponent _, InteractionAttemptFailed args)
    {
        RaiseLocalEvent(uid, new BackflipActionEvent());
    }

    private void OnHungerModified(EntityUid uid, SpaceFerretComponent comp, HungerModifiedEvent args)
    {
        if (_mind.TryGetObjectiveComp<ConsumeNutrientsConditionComponent>(uid, out var nutrientsCondition) && args.Amount > 0)
        {
            nutrientsCondition.NutrientsConsumed += args.Amount;
        }
    }
}
