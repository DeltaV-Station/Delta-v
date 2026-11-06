using Content.Client.Message;
using Content.Client.Stylesheets;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;

using Robust.Shared.Network;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

using Content.Shared._DV.MultipleUiKey;

namespace Content.Client._DV.MultipleUiKey;

public sealed class MultipleUiKeyStatusControl : Control
{
    private readonly MultipleUiKeyComponent _comp;
    private readonly RichTextLabel _label;

    public MultipleUiKeyStatusControl(MultipleUiKeyComponent comp)
    {
        _comp = comp;
        _label = new RichTextLabel { StyleClasses = { StyleClass.ItemStatus } };
        UpdateLabel();
        AddChild(_label);
        return;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (_comp.LabelUpdateNeeded)
        {
            _comp.LabelUpdateNeeded = false;
            UpdateLabel();
        }

        return;
    }

    public void UpdateLabel()
    {
        _label.SetMarkup(_comp.ShowCurrentMode ? Loc.GetString(_comp.Modes[_comp.Index].Name) : string.Empty);
        return;
    }
}
