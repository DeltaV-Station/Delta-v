using Content.Client._DV.PDA;
using Content.Client.Stylesheets;
using Content.Client.Message;
using System.Linq;

namespace Content.Client.PDA;

public sealed partial class PdaMenu
{
    public event Action<string>? OnUnlinkDevicePressed;

    private string _evacStatus = Loc.GetString("comp-pda-ui-unknown");
    private TimeSpan? _evacArrivalTime = null;
    private TimeSpan? _evacDepartureTime = null;

    public void UpdateLinkedDevices(Dictionary<string, string> devices)
    {
        LinkedDeviceList.RemoveAllChildren();

        var even = false;
        foreach (var (address, name) in devices.OrderBy(kvp => kvp.Value))
        {
            var item = new LinkedDeviceItem()
            {
                DeviceName = { Text = name },
            };
            item.Unlink.OnPressed += _ => OnUnlinkDevicePressed?.Invoke(address);
            LinkedDeviceList.AddChild(item);
            LinkedDeviceList.StyleClasses.Add(even ? StyleClass.PanelDark : StyleClass.PanelLight);
            even = !even;
        }
    }

    private void UpdateEvacStatus()
    {
        if (_evacDepartureTime is { } evacDepartureTime)
        {
            var diff = MathHelper.Max(evacDepartureTime.Subtract(_gameTiming.CurTime), TimeSpan.Zero);
            if (diff == TimeSpan.Zero)
                _evacStatus = Loc.GetString("comp-pda-ui-evac-gone");
            else
                _evacStatus = Loc.GetString("comp-pda-ui-evac-etd", ("time", diff.ToString("mm\\:ss")));
        }
        else
        {
            if (_evacArrivalTime is { } evacArrivalTime)
            {
                var diff = MathHelper.Max(evacArrivalTime.Subtract(_gameTiming.CurTime), TimeSpan.Zero);
                _evacStatus = Loc.GetString("comp-pda-ui-evac-eta", ("time", diff.ToString("mm\\:ss")));
            }
            else
            {
                _evacStatus = Loc.GetString("comp-pda-ui-evac-not-called");
            }
        }

        StationEvacStatusLabel.SetMarkup(Loc.GetString("comp-pda-ui-evac-status", ("status", _evacStatus)));
    }
}
