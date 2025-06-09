using Content.Shared.FixedPoint;
using Content.Shared.Whitelist; // imp
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Chemistry.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ReagentTankComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 TransferAmount { get; set; } = FixedPoint2.New(10);

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ReagentTankType TankType { get; set; } = ReagentTankType.Unspecified;

    // imp start
    [DataField]
    public EntityWhitelist? FuelWhitelist;

    [DataField]
    public EntityWhitelist? FuelBlacklist;
    // imp end
}

[Serializable, NetSerializable]
public enum ReagentTankType : byte
{
    Unspecified,
    Fuel
}
