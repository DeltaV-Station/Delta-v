using Content.Client._DV.Abilities.Psionics;
using Content.Client.Eui;
using JetBrains.Annotations;

namespace Content.Client._DV.Psionics.UI;

[UsedImplicitly]
public sealed class EruptionWarningEui : BaseEui
{
    private readonly EruptionWarning _window;

    public EruptionWarningEui()
    {
        _window = new EruptionWarning();
    }

    public override void Opened()
    {
        _window.OpenCentered();
    }

    public override void Closed()
    {
        base.Closed();

        _window.Close();
    }

}
