using Content.Shared.Security;
using Robust.Shared.Serialization;

namespace Content.Shared.CartridgeLoader.Cartridges;

/// <summary>
/// </summary>
[Serializable, NetSerializable]
public sealed class MailMetricUiState : BoundUserInterfaceState
{
    public readonly int MailEarnings;
    public MailMetricUiState(int mailEarnings)
    {
        MailEarnings = mailEarnings;
    }
}
