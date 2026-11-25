using Content.Client._DV.PDA;
using Content.Client.Stylesheets;
using System.Linq;

namespace Content.Client.PDA;

public sealed partial class PdaMenu
{
    public event Action<string>? OnUnlinkDevicePressed;

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
}
