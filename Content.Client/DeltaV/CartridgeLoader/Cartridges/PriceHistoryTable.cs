using System.Linq;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.DeltaV.CartridgeLoader.Cartridges;

public sealed class PriceHistoryTable : BoxContainer
{
    private readonly GridContainer _grid;
    private static readonly Color PositiveColor = Color.FromHex("#00ff00");
    private static readonly Color NegativeColor = Color.FromHex("#ff0000");
    private static readonly Color NeutralColor = Color.FromHex("#ffffff");

    public PriceHistoryTable()
    {
        Orientation = LayoutOrientation.Vertical;
        HorizontalExpand = true;
        Margin = new Thickness(0, 5, 0, 0);

        // Create header
        var header = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            HorizontalExpand = true,
        };

        header.AddChild(new Label
        {
            Text = "Price History",
            HorizontalExpand = true,
            StyleClasses = { "LabelSubText" }
        });

        AddChild(header);

        // Create grid for price history
        _grid = new GridContainer
        {
            Columns = 5, // Display 5 entries per row
            HorizontalExpand = true,
        };

        AddChild(_grid);
    }

    public void Update(List<float> priceHistory)
    {
        _grid.RemoveAllChildren();

        // Take last 10 prices as per StockMarketSystem
        var lastTenPrices = priceHistory.TakeLast(10).ToList();

        for (var i = 0; i < lastTenPrices.Count; i++)
        {
            var price = lastTenPrices[i];
            var previousPrice = i > 0 ? lastTenPrices[i - 1] : price;
            var priceChange = ((price - previousPrice) / previousPrice) * 100;

            var entryContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                MinWidth = 80,
                Margin = new Thickness(2),
            };

            var priceLabel = new Label
            {
                Text = $"${price:F2}",
                HorizontalAlignment = HAlignment.Right,
            };

            var changeLabel = new Label
            {
                Text = $"{(priceChange >= 0 ? "+" : "")}{priceChange:F2}%",
                HorizontalAlignment = HAlignment.Right,
                StyleClasses = { "LabelSubText" },
                Modulate = priceChange switch
                {
                    > 0 => PositiveColor,
                    < 0 => NegativeColor,
                    _ => NeutralColor,
                }
            };

            entryContainer.AddChild(priceLabel);
            entryContainer.AddChild(changeLabel);
            _grid.AddChild(entryContainer);
        }
    }
}
