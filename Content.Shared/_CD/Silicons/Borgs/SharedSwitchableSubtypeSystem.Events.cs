using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.Silicons.Borgs;

[Serializable, NetSerializable]
public sealed class BorgSelectSubtypeMessage(ProtoId<EntityPrototype>? subtype) : BoundUserInterfaceMessage
{
    public ProtoId<EntityPrototype>? Subtype = subtype;
}

[ByRefEvent]
public record struct AfterBorgTypeSelectEvent;

[ByRefEvent]
public record struct TypeTryingToUpdateVisualsEvent;
