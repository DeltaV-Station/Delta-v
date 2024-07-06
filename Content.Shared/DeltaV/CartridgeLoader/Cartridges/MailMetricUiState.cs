using Content.Shared.Security;
using Robust.Shared.Serialization;

namespace Content.Shared.CartridgeLoader.Cartridges;

/// <summary>
/// </summary>
[Serializable, NetSerializable]
public sealed class MailMetricUiState : BoundUserInterfaceState
{
    public readonly int MailEarnings;
    public readonly int DamagedMailLosses;
    public readonly int ExpiredMailLosses;
    public readonly int TamperedMailLosses;
    public readonly int OpenedMailCount;
    public readonly int DamagedMailCount;
    public readonly int ExpiredMailCount;
    public readonly int TamperedMailCount;
    public readonly int UnopenedMailCount;

    public int TotalMail { get; }
    public int TotalIncome { get; }
    public double SuccessRate { get; }

    public MailMetricUiState(int mailEarnings,
                             int damagedMailLosses,
                             int expiredMailLosses,
                             int tamperedMailLosses,
                             int openedMailCount,
                             int damagedMailCount,
                             int expiredMailCount,
                             int tamperedMailCount,
                             int unopenedMailCount)
    {
        MailEarnings = mailEarnings;
        DamagedMailLosses = damagedMailLosses;
        ExpiredMailLosses = expiredMailLosses;
        TamperedMailLosses = tamperedMailLosses;
        OpenedMailCount = openedMailCount;
        DamagedMailCount = damagedMailCount;
        ExpiredMailCount = expiredMailCount;
        TamperedMailCount = tamperedMailCount;
        UnopenedMailCount = unopenedMailCount;

        TotalMail = openedMailCount + unopenedMailCount;
        TotalIncome = mailEarnings - damagedMailLosses - expiredMailLosses - tamperedMailLosses;
        SuccessRate = TotalMail > 0 ?
            Math.Round((double) openedMailCount / TotalMail * 100, 2)
            : 0;
    }
}

