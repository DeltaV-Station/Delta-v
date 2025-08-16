using Content.Client.Eui;

namespace Content.Client._DV.CosmicCult.UI;

public sealed class CosmicUnempoweredEui : BaseEui
{
    private readonly CosmicUnempoweredMenu _menu;

    public CosmicUnempoweredEui()
    {
        _menu = new CosmicUnempoweredMenu();
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
