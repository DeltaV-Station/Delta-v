using Content.Shared.Cargo;

namespace Content.Server.DeltaV.Cargo.Components;

/// <summary>
/// Added to the abstract representation of a station to track stats related to mail delivery and income
/// </summary>
[RegisterComponent, Access(typeof(SharedCargoSystem))]
public sealed partial class StationLogisticStatsComponent : Component
{
    [DataField]
    public int MailEarnings;

    [DataField]
    public int DamagedMailLosses;

    [DataField]
    public int ExpiredMailLosses;

    [DataField]
    public int TamperedMailLosses;

    [DataField]
    public int OpenedMailCount;

    [DataField]
    public int DamagedMailCount;

    [DataField]
    public int ExpiredMailCount;

    [DataField]
    public int TamperedMailCount;
}
