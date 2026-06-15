using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Generic;

namespace Content.Shared._DV.Uprising;

[RegisterComponent]
public sealed partial class CommandStatusObjectiveComponent : Component
{
    [DataField(required: true, customTypeSerializer: typeof(CustomHashSetSerializer<string, ComponentNameSerializer>))]
    public HashSet<string> ConvertedRoles;

    [DataField]
    public bool ShouldUnconvertedBeIncapacitated;
}
