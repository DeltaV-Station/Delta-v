using Content.Client.UserInterface.Controls;
using Content.Shared._DV.Surgery;
using Content.Shared.FixedPoint;
using Robust.Shared.Timing;

namespace Content.Client._DV.Surgery;

public sealed class SurgeryDirtinessItemStatus : SplitBar
{
    private readonly IEntityManager _entMan;
    private readonly EntityUid _uid;
    private FixedPoint2? _dirtiness = null;

    public SurgeryDirtinessItemStatus(EntityUid uid, IEntityManager entManager)
    {
        _uid = uid;
        _entMan = entManager;
        Margin = new Thickness(4);
        MinHeight = 16;
        MaxHeight = 16;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);
        if (!_entMan.TryGetComponent<SurgeryDirtinessComponent>(_uid, out var dirtiness))
            return;

        if (_dirtiness == dirtiness.Dirtiness)
            return;
        _dirtiness = dirtiness.Dirtiness;

        Clear();

        var amount = FixedPoint2.Min(_dirtiness.Value / 100, 1);
        var remaining = 1 - amount;

        if (amount != FixedPoint2.Zero)
            AddEntry(amount.Float(), amount < 0.5 ? Color.RosyBrown : Color.Red);

        if (remaining != FixedPoint2.Zero)
            AddEntry(remaining.Float(), Color.SlateGray);
    }
}
