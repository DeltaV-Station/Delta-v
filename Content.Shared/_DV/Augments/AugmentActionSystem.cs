using Content.Shared._Shitmed.Body.Events;
using Content.Shared.Actions;

namespace Content.Shared._DV.Augments;

public sealed class AugmentActionSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AugmentActionComponent, MechanismEnabledEvent>(OnEnabled);
        SubscribeLocalEvent<AugmentActionComponent, MechanismDisabledEvent>(OnDisabled);
    }

    private void OnEnabled(Entity<AugmentActionComponent> augment, ref MechanismEnabledEvent args)
    {
        var ev = new GetItemActionsEvent(_actionContainer, args.Body, augment);
        RaiseLocalEvent(augment, ev);

        _actions.GrantActions(args.Body, ev.Actions, augment);
    }

    private void OnDisabled(Entity<AugmentActionComponent> augment, ref MechanismDisabledEvent args)
    {
        _actions.RemoveProvidedActions(args.Body, augment);
    }
}
