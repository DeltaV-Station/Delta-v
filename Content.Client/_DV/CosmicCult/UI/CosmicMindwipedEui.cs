using Content.Client.Eui;

namespace Content.Client._DV.CosmicCult.UI;

public sealed class CosmicMindwipedEui : BaseEui
{
    private readonly CosmicMindwipedMenu _menu;

    public CosmicMindwipedEui()
    {
        _menu = new CosmicMindwipedMenu();
    }

    public override void Opened()
    {
        _menu.OpenCentered();
    }

    public override void Closed()
    {
        base.Closed();

        _menu.Close();
    }
}
