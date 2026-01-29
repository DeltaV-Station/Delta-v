using Content.Client._DV.UserInterfaces.BuildInfo.Controls;
using JetBrains.Annotations;
using Robust.Client.UserInterface.Controllers;

namespace Content.Client._DV.UserInterfaces.BuildInfo;

[UsedImplicitly]
public sealed class BuildInfoUIController : UIController
{
    private BuildInfoWindow _changeLogWindow = default!;

    public void OpenWindow()
    {
        EnsureWindow();

        _changeLogWindow.OpenCentered();
        _changeLogWindow.MoveToFront();
    }

    private void EnsureWindow()
    {
        if (_changeLogWindow is { Disposed: false })
        {
            // return; ADD THIS BACK WHEN I DON'T NEED HOT RELOAD
        }

        _changeLogWindow = UIManager.CreateWindow<BuildInfoWindow>();
    }

    public void ToggleWindow()
    {
        if (_changeLogWindow is { IsOpen: true })
        {
            _changeLogWindow.Close();
        }
        else
        {
            OpenWindow();
        }
    }
}
