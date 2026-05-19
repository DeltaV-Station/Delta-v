using Content.Shared._Shitmed.Body.Organ;
using Content.Shared.Actions;
using Content.Shared.Body.Organ;
using Robust.Shared.GameObjects;

namespace Content.Shared._DV.Augments;

public sealed class AugmentActivatableUISystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _uiSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AugmentActivatableUIComponent, AugmentUIOpenEvent>(OnOpen);
        SubscribeLocalEvent<AugmentActivatableUIComponent, GetItemActionsEvent>(OnGetActions);
    }

    private void OnOpen(Entity<AugmentActivatableUIComponent> augment, ref AugmentUIOpenEvent args)
    {
        if (!TryComp<OrganComponent>(augment, out var organ) || organ.Body is not {} body)
            return;

        if (augment.Comp.Key == null || !_uiSystem.HasUi(augment, augment.Comp.Key))
            return;

        _uiSystem.OpenUi(augment.Owner, augment.Comp.Key, body);
        args.Handled = true;
    }

    private void OnGetActions(Entity<AugmentActivatableUIComponent> ent, ref GetItemActionsEvent args)
    {
        if (!TryComp<OrganComponent>(ent, out var organ) || organ.Body is not {} body)
            return;

        args.AddAction(ref ent.Comp.OpenActionEntity, ent.Comp.OpenAction);
    }
}
