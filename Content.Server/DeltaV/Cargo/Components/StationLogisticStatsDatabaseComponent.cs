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
}
