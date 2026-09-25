using Content.Shared.Actions;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Content.Shared.Roles.Components;

namespace Content.Shared.Uprising;

public sealed class ConversionOfferActionSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MindRoleComponent, ConversionOfferActionEvent>(OnOffer);
    }

    private void OnOffer(Entity<MindRoleComponent> ent, ref ConversionOfferActionEvent args)
    {
        if (_mind.GetMind(args.Target) is not { } mind)
            return;

        foreach (var incompatible in args.IncompatibleMindRoleTypes)
        {
            if (_role.MindHasRole(mind, incompatible, out _))
                return;
        }

        _actions.AddAction(args.Target, args.AcceptAction);
        args.Handled = true;
    }
}
