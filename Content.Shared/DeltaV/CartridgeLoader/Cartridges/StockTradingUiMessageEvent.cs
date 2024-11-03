using Robust.Shared.Serialization;

namespace Content.Shared.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class StockTradingUiMessageEvent : CartridgeMessageEvent
{
    public readonly StockTradingUiAction Action;
    public readonly string Company;
    public readonly float Amount;

    public StockTradingUiMessageEvent(StockTradingUiAction action, string company, float amount)
    {
        Action = action;
        Company = company;
        Amount = amount;
    }
}

[Serializable, NetSerializable]
public enum StockTradingUiAction
{
    Buy,
    Sell,
}
