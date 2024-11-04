using System.Linq;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.DeltaV.CartridgeLoader.Cartridges;

public sealed class PriceHistoryTable : BoxContainer
{
    private readonly GridContainer _grid;

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

        // Create a panel container with styled background
        var panel = new PanelContainer
        {
            HorizontalExpand = true,
            Margin = new Thickness(0, 2, 0, 0)
        };

        // Create and apply the style
        var styleBox = new StyleBoxFlat
        {
            BackgroundColor = StockTradingUiFragment.BackgroundColor,
            ContentMarginLeftOverride = 6,
            ContentMarginRightOverride = 6,
            ContentMarginTopOverride = 4,
            ContentMarginBottomOverride = 4,
            BorderColor = StockTradingUiFragment.BorderColor,
            BorderThickness = new Thickness(1),
        };

        panel.PanelOverride = styleBox;

        // Create a centering container
        var centerContainer = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            HorizontalExpand = true,
            HorizontalAlignment = HAlignment.Center,
        };

        // Create grid for price history
        _grid = new GridContainer
        {
            Columns = 5, // Display 5 entries per row
        };

        centerContainer.AddChild(_grid);
        panel.AddChild(centerContainer);
        AddChild(panel);
    }

    public void Update(List<float> priceHistory)
    {
        _grid.RemoveAllChildren();

        // Take last 5 prices
        var lastFivePrices = priceHistory.TakeLast(5).ToList();

        for (var i = 0; i < lastFivePrices.Count; i++)
        {
            var price = lastFivePrices[i];
            var previousPrice = i > 0 ? lastFivePrices[i - 1] : price;
            var priceChange = ((price - previousPrice) / previousPrice) * 100;

            var entryContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                MinWidth = 80,
                HorizontalAlignment = HAlignment.Center,
            };

            var priceLabel = new Label
            {
                Text = $"${price:F2}",
                HorizontalAlignment = HAlignment.Center,
            };

            var changeLabel = new Label
            {
                Text = $"{(priceChange >= 0 ? "+" : "")}{priceChange:F2}%",
                HorizontalAlignment = HAlignment.Center,
                StyleClasses = { "LabelSubText" },
                Modulate = priceChange switch
                {
                    > 0 => StockTradingUiFragment.PositiveColor,
                    < 0 => StockTradingUiFragment.NegativeColor,
                    _ => StockTradingUiFragment.NeutralColor,
                }
            };

            entryContainer.AddChild(priceLabel);
            entryContainer.AddChild(changeLabel);
            _grid.AddChild(entryContainer);
        }
    }
}
