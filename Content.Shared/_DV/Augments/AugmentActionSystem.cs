using Content.Shared._Shitmed.Body.Organ;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Body.Organ;

namespace Content.Shared._DV.Augments;

public sealed class AugmentActionSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AugmentActionComponent, OrganEnableChangedEvent>(OnOrganEnableChanged);
    }

    private void OnOrganEnableChanged(Entity<AugmentActionComponent> augment, ref OrganEnableChangedEvent args)
    {
        if (!TryComp<OrganComponent>(augment, out var organ) || organ.Body is not {} body)
            return;

        var actionsComponent = EnsureComp<ActionsComponent>(body);

        if (args.Enabled)
        {
            var ev = new GetItemActionsEvent(_actionContainer, body, augment);
            RaiseLocalEvent(augment, ev);

            _actions.GrantActions(body, ev.Actions, augment.Owner);
        }
        else
        {
            _actions.RemoveProvidedActions(body, augment, actionsComponent);
        }
    }
}
