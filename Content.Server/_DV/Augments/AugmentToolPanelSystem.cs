using System.Linq;
using Content.Server.Power.EntitySystems;
using Content.Shared._DV.Augments;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Containers;

namespace Content.Server._DV.Augments;

public sealed class AugmentToolPanelSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedStorageSystem _storage = default!;
    [Dependency] private readonly AugmentPowerCellSystem _augmentPowerCell = default!;

    public override void Initialize()
    {
        base.Initialize();

        Subs.BuiEvents<AugmentToolPanelComponent>(AugmentToolPanelUiKey.Key, subs =>
        {
            subs.Event<AugmentToolPanelSystemMessage>(OnSwitchTool);
        });
    }

    private void OnSwitchTool(Entity<AugmentToolPanelComponent> augment, ref AugmentToolPanelSystemMessage args)
    {
        if (!TryComp<OrganComponent>(augment, out var organ) || organ.Body is not {} body)
            return;

        if (!TryComp<HandsComponent>(body, out var hands))
            return;

        if (!_container.TryGetContainingContainer(augment.Owner, out var container))
            return;

        if (!_augmentPowerCell.TryDrawPower(augment, augment.Comp.PowerDrawOnSwitch))
            return;

        foreach (var part in _body.GetBodyPartChildren(container.Owner))
        {
            if (part.Component.PartType != BodyPartType.Hand)
                continue;

            var handLocation = part.Component.Symmetry switch {
                BodyPartSymmetry.None => HandLocation.Middle,
                BodyPartSymmetry.Left => HandLocation.Left,
                BodyPartSymmetry.Right => HandLocation.Right,
                _ => throw new InvalidOperationException(),
            };

            var desiredHand = hands.Hands.Values.FirstOrDefault(hand => hand.Location == handLocation);
            if (desiredHand == null)
                continue;

            // if we have a tool that's currently out
            if (HasComp<AugmentToolPanelActiveItemComponent>(desiredHand.HeldEntity))
            {
                // deposit it back into the storage
                RemComp<AugmentToolPanelActiveItemComponent>(desiredHand.HeldEntity!.Value);

                if (!_storage.PlayerInsertEntityInWorld(augment.Owner, body, desiredHand.HeldEntity!.Value))
                {
                    EnsureComp<AugmentToolPanelActiveItemComponent>(desiredHand.HeldEntity!.Value);
                    return;
                }
            }
            else if (desiredHand.HeldEntity is not null)
            {
                _popup.PopupCursor(Loc.GetString("augment-tool-panel-hand-full"), body);
                return;
            }

            if (GetEntity(args.DesiredTool) is not {} desiredTool)
                return;

            if (!_hands.TryPickup(body, desiredTool, desiredHand))
            {
                _popup.PopupCursor(Loc.GetString("augment-tool-panel-cannot-pick-up"), body);
                return;
            }
            EnsureComp<AugmentToolPanelActiveItemComponent>(desiredTool);
        }
    }
}
