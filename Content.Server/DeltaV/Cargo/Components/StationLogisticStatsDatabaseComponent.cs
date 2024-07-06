using Content.Shared.Cargo;

namespace Content.Server.DeltaV.Cargo.Components;

/// <summary>
/// Added to the abstract representation of a station to track stats related to mail delivery and income
/// </summary>
[RegisterComponent, Access(typeof(SharedCargoSystem))]
public sealed partial class StationLogisticStatsComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("mailEarnings")]
    public int MailEarnings = 0;

    [ViewVariables(VVAccess.ReadWrite), DataField("damagedMailLosses")]
    public int DamagedMailLosses = 0;

    [ViewVariables(VVAccess.ReadWrite), DataField("expiredMailLosses")]
    public int ExpiredMailLosses = 0;

    [ViewVariables(VVAccess.ReadWrite), DataField("tamperedMailLosses")]
    public int TamperedMailLosses = 0;

    [ViewVariables(VVAccess.ReadWrite), DataField("openedMailCount")]
    public int OpenedMailCount = 0;

    [ViewVariables(VVAccess.ReadWrite), DataField("damagedMailCount")]
    public int DamagedMailCount = 0;

    [ViewVariables(VVAccess.ReadWrite), DataField("expiredMailCount")]
    public int ExpiredMailCount = 0;

    [ViewVariables(VVAccess.ReadWrite), DataField("tamperedMailCount")]
    public int TamperedMailCount = 0;
}
