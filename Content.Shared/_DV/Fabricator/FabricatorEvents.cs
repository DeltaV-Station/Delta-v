using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Fabricator;

[Serializable, NetSerializable]
public sealed class FabricatorStartMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class FabricatorEjectMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class FabricatorEjectIndexedMessage(NetEntity ent) : BoundUserInterfaceMessage
{
    public NetEntity Target = ent;
}

[Serializable, NetSerializable]
public sealed class FabricatorUpdateUserInterfaceState(
    (NetEntity, int)[] solids,
    Dictionary<ProtoId<ReagentPrototype>, FixedPoint2> reagents,
    bool busy,
    TimeSpan endTime)
    : BoundUserInterfaceState
{
    public (NetEntity, int)[] ContainedSolids = solids;
    public Dictionary<ProtoId<ReagentPrototype>, FixedPoint2> ContainedReagents = reagents;
    public bool Busy = busy;
    public TimeSpan EndTime = endTime;
}
