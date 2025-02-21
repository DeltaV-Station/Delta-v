using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Lock;
using Content.Shared.ActionBlocker;
using Content.Shared.Popups;
using JetBrains.Annotations;

namespace Content.Shared._DV.Lock;

/// <summary>
/// Handles (un)locking and examining of Lock components
/// </summary>
[UsedImplicitly]
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

    private void OnActivateAttempt(EntityUid uid, ItemToggleRequiresLockComponent component, ref ItemToggleActivateAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (TryComp<LockComponent>(uid, out var lockComp) && lockComp.Locked != component.RequireLocked)
        {
            args.Cancelled = true;
            if (lockComp.Locked)
                args.Popup = Loc.GetString("lock-comp-has-user-access-fail");
        }
    }
}
