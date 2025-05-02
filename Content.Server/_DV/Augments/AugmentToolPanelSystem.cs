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
using Robust.Shared.Audio.Systems;

namespace Content.Server._DV.Augments;

public sealed class AugmentToolPanelSystem : SharedAugmentToolPanelSystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedStorageSystem _storage = default!;
    [Dependency] private readonly AugmentPowerCellSystem _augmentPowerCell = default!;
    [Dependency] private readonly AugmentSystem _augment = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AugmentToolPanelComponent, AugmentPowerLostEvent>(OnPowerLost);
        Subs.BuiEvents<AugmentToolPanelComponent>(AugmentToolPanelUiKey.Key, subs =>
        {
            subs.Event<AugmentToolPanelSystemMessage>(OnSwitchTool);
        });
    }

    private void OnPowerLost(Entity<AugmentToolPanelComponent> ent, ref AugmentPowerLostEvent args)
    {
        if (ent.Comp.SelectedTool is not {} item)
            return;

        // deposit held tool into storage if power is lost
        RemComp<AugmentToolPanelActiveItemComponent>(item);

        if (!_storage.PlayerInsertEntityInWorld(ent.Owner, args.Body, item))
        {
            EnsureComp<AugmentToolPanelActiveItemComponent>(item);
            return;
        }

        SetTool(ent, args.Body, null);
    }

    private void OnSwitchTool(Entity<AugmentToolPanelComponent> augment, ref AugmentToolPanelSystemMessage args)
    {
        if (_body.GetBody(augment) is not {} body)
            return;

        if (!TryComp<HandsComponent>(body, out var hands))
            return;

        if (!_augmentPowerCell.TryDrawPower(augment, augment.Comp.ChargeUseOnSwitch))
            return;

        foreach (var part in _body.GetBodyChildrenOfType(body, BodyPartType.Hand))
        {
            var handLocation = part.Component.Symmetry switch {
                BodyPartSymmetry.Left => HandLocation.Left,
                BodyPartSymmetry.Right => HandLocation.Right,
                _ => HandLocation.Middle
            };

            var desiredHand = hands.Hands.Values.FirstOrDefault(hand => hand.Location == handLocation);
            if (desiredHand == null)
                continue;

            // try to stash the held tool when deselecting
            if (desiredHand.HeldEntity is {} item)
            {
                // if we have a tool that's currently out
                if (HasComp<AugmentToolPanelActiveItemComponent>(item))
                {
                    // deposit it back into the storage
                    RemComp<AugmentToolPanelActiveItemComponent>(item);

                    if (!_storage.PlayerInsertEntityInWorld(augment.Owner, body, item))
                    {
                        EnsureComp<AugmentToolPanelActiveItemComponent>(item);
                        return;
                    }

                    SetTool(augment, body, null);
                }
                else
                {
                    // can't select a tool with a random item in-hand
                    _popup.PopupCursor(Loc.GetString("augment-tool-panel-hand-full"), body);
                    return;
                }
            }

            if (GetEntity(args.DesiredTool) is not {} desiredTool)
                return;

            if (!_hands.TryPickup(body, desiredTool, desiredHand))
            {
                _popup.PopupCursor(Loc.GetString("augment-tool-panel-cannot-pick-up"), body);
                return;
            }

            EnsureComp<AugmentToolPanelActiveItemComponent>(desiredTool);
            SetTool(augment, body, desiredTool);
        }
    }

    private void SetTool(Entity<AugmentToolPanelComponent> ent, EntityUid body, EntityUid? tool)
    {
        if (ent.Comp.SelectedTool == tool)
            return;

        ent.Comp.SelectedTool = tool;
        Dirty(ent);
        _augment.UpdateBodyDraw(body);
        _audio.PlayPvs(ent.Comp.SwitchSound, ent);
    }
}
