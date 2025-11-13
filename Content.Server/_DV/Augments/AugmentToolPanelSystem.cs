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

            // god, I hate this hand refactor so much. This will be kind of messy since we can't get the ID of the hand
            // from the hand entity. Im so sorry.
            var desiredHandId = hands.Hands.Keys.FirstOrDefault(id => hands.Hands[id] == desiredHand);

            _hands.TryGetHeldItem((body, hands), desiredHandId, out var heldItem);

            // if we have a tool that's currently out
            if (HasComp<AugmentToolPanelActiveItemComponent>(heldItem))
            {
                // deposit it back into the storage
                RemComp<AugmentToolPanelActiveItemComponent>(heldItem!.Value);

                if (!_storage.PlayerInsertEntityInWorld(augment.Owner, body, heldItem!.Value))
                {
                    EnsureComp<AugmentToolPanelActiveItemComponent>(heldItem!.Value);
                    return;
                }
            }
            else if (heldItem is not null)
            {
                _popup.PopupCursor(Loc.GetString("augment-tool-panel-hand-full"), body);
                return;
            }

            if (GetEntity(args.DesiredTool) is not {} desiredTool)
                return;

            if (!_hands.TryPickup(body, desiredTool, desiredHandId))
            {
                _popup.PopupCursor(Loc.GetString("augment-tool-panel-cannot-pick-up"), body);
                return;
            }
            EnsureComp<AugmentToolPanelActiveItemComponent>(desiredTool);
        }
    }
}
