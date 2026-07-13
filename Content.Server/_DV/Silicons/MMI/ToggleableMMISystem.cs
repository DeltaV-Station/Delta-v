using Content.Server.Ghost.Roles.Components;
using Content.Server.Ghost.Roles;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;

namespace Content.Server._DV.Silicons.MMI;

public sealed class ToggleableMMISystem : EntitySystem
{
    [Dependency] private readonly ToggleableGhostRoleSystem _ghost = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ToggleableMMIComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<ToggleableMMIComponent, ItemSlotEjectAttemptEvent>(OnEjectAttempt);
    }

    // MMI must have brain for <see cref="ToggleableGhostRole"/>, otherwise block UseInHand.
    private void OnUseInHand(EntityUid uid, ToggleableMMIComponent comp, UseInHandEvent args)
    {
        args.Handled = true;

        if (!TryComp<MMIComponent>(uid, out var mmi))
            return;

        if (!TryComp(uid, out ContainerManagerComponent? containers))
            return;

        if (!containers.TryGetContainer(mmi.BrainSlotId, out var container) ||
            container.ContainedEntities.Count == 0)
            return;

        args.Handled = false;
    }

    // Prevent ejecting the brain while "searching".
    private void OnEjectAttempt(EntityUid uid, ToggleableMMIComponent comp, ref ItemSlotEjectAttemptEvent args)
    {
        if (HasComp<GhostTakeoverAvailableComponent>(uid))
        {
            args.Cancelled = true;

            _popupSystem.PopupEntity(Loc.GetString("mmi-searching-cannot-eject"), uid, PopupType.Medium);
        }
    }
}
