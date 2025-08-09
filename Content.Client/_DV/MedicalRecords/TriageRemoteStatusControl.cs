using Content.Client.Message;
using Content.Client.Stylesheets;
using Content.Shared._DV.MedicalRecords;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;

namespace Content.Client._DV.MedicalRecords;

public sealed class TriageRemoteStatusControl : Control
{
    private readonly TriageRemoteComponent _parent;
    private readonly RichTextLabel _label;
    private OperatingMode? _previousStatus = null;

    public TriageRemoteStatusControl(TriageRemoteComponent parent)
    {
        _parent = parent;
        _label = new RichTextLabel { StyleClasses = { StyleNano.StyleClassItemStatus } };
        AddChild(_label);
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        // We need to not return for the first time, so it updates once when picking up
        if (_previousStatus == _parent.Mode && !_previousStatus.HasValue)
        {
            return;
        }

        _previousStatus = _parent.Mode;

        // todo localize this!
        _label.SetMarkup(_parent.Mode switch
        {
            OperatingMode.GiveLow => "Give: Low Priority",
            OperatingMode.GiveDnr => "Give: DNR",
            OperatingMode.GiveHigh => "Give: High Priority",
            _ => "Give: Unknown",
        });
    }
}
