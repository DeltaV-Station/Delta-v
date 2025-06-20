using System.Numerics;
using Content.Client.UserInterface.Controls;
using Content.Shared._DV.Surgery;
using Content.Shared._Shitmed.Medical.Surgery.Tools;
using Content.Shared.FixedPoint;
using Content.Shared.Inventory;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Client._DV.Surgery;

public sealed class SurgeryDirtinessItemStatus : SplitBar
{
    private readonly IEntityManager _entMan;
    private readonly EntityUid _uid;
    private readonly InventorySystem _inventory;
    private readonly SharedContainerSystem _container;
    private FixedPoint2? _dirtiness = null;
    private FixedPoint2? _gloveDirtiness = null;

    private static readonly Color SelfCleanColor = new Color(0xD1, 0xD5, 0xD9);
    private static readonly Color SelfDirtyColor = new Color(0xE9, 0x3D, 0x58);
    private static readonly Color GloveCleanColor = new Color(0xAB, 0xE9, 0xFB);
    private static readonly Color GloveDirtyColor = new Color(0xE9, 0x64, 0x3A);

    public SurgeryDirtinessItemStatus(EntityUid uid, IEntityManager entMan, InventorySystem inventory, SharedContainerSystem container)
    {
        _uid = uid;
        _entMan = entMan;
        _inventory = inventory;
        _container = container;
        MinBarSize = new Vector2(10, 0);
        Margin = new Thickness(4);
        MinHeight = 16;
        MaxHeight = 16;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (!_entMan.TryGetComponent<SurgeryDirtinessComponent>(_uid, out var comp))
            return;

        var isTool = _entMan.HasComponent<SurgeryToolComponent>(_uid);

        if (_dirtiness == comp.Dirtiness && !isTool)
            return;
        _dirtiness = comp.Dirtiness;

        if (isTool && _container.TryGetContainingContainer((_uid, null, null), out var container))
        {
            var user = container.Owner;

            FixedPoint2? glovesDirtiness =
                _inventory.TryGetSlotEntity(user, "gloves", out var gloves)
                    ? _entMan.TryGetComponent<SurgeryDirtinessComponent>(_uid, out var glovesComp)
                        ? glovesComp.Dirtiness
                        : FixedPoint2.Zero
                    : null;

            if (glovesDirtiness is not null && _gloveDirtiness == glovesDirtiness)
                return;
            _gloveDirtiness = glovesDirtiness;

            Clear();

            var toolAmount = FixedPoint2.Min(_dirtiness.Value / 100, 1);
            var gloveAmount = FixedPoint2.Min((_gloveDirtiness ?? 0) / 100, 1);
            var remaining = 1 - toolAmount - gloveAmount;

            var clean = (_dirtiness + (_gloveDirtiness ?? 0)) < 50;
            var maskOn = _inventory.TryGetSlotEntity(user, "mask", out var _);
            var isFine = clean && maskOn && _gloveDirtiness is not null;

            if (toolAmount != FixedPoint2.Zero)
                AddEntry(toolAmount.Float() * 100, isFine ? SelfCleanColor : SelfDirtyColor);

            if (gloveAmount != FixedPoint2.Zero)
                AddEntry(gloveAmount.Float() * 100, isFine ? GloveCleanColor : GloveDirtyColor);

            if (remaining > FixedPoint2.Zero)
                AddEntry(remaining.Float() * 100, Color.SlateGray);
        }
        else
        {
            Clear();

            var amount = FixedPoint2.Min(_dirtiness.Value / 100, 1);
            var remaining = 1 - amount;

            if (amount != FixedPoint2.Zero)
                AddEntry(amount.Float(), amount < 0.5 ? SelfCleanColor : SelfDirtyColor);

            if (remaining != FixedPoint2.Zero)
                AddEntry(remaining.Float(), Color.SlateGray);
        }
    }
}
