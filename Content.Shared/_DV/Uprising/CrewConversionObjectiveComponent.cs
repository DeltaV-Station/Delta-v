using Content.Shared.NPC.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Generic;

namespace Content.Shared._DV.Uprising;

[RegisterComponent]
public sealed partial class CrewConversionObjectiveComponent : Component
{
    [DataField(required: true, customTypeSerializer: typeof(CustomHashSetSerializer<string, ComponentNameSerializer>))]
    public HashSet<string> ConvertedRoles;

    [DataField(required: true)]
    public HashSet<ProtoId<NpcFactionPrototype>> ConversionSourceFactions;

    [DataField(required: true)]
    public float TargetFraction;
}
