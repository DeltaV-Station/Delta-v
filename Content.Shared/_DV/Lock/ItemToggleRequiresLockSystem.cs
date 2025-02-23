using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Lock;
using Content.Shared.ActionBlocker;
using Content.Shared.Popups;

namespace Content.Shared._DV.Lock;

public sealed class ItemToggleRequiresLockSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly SharedPopupSystem _sharedPopupSystem = default!;

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ItemToggleRequiresLockComponent, ItemToggleActivateAttemptEvent>(OnActivateAttempt);
    }

    private void OnActivateAttempt(Entity<T> ent, ItemToggleRequiresLockComponent comp, ref ItemToggleActivateAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (TryComp<LockComponent>(ent, out var lockComp) && lockComp.Locked != ent.Comp.RequireLocked)
        {
            args.Cancelled = true;
            if (lockComp.Locked)
                args.Popup = Loc.GetString("lock-comp-has-user-access-fail");
        }
    }
}
