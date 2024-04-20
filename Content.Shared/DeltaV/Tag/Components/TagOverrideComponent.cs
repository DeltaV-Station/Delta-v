using Content.Shared.Tag;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;

namespace Content.Shared.DeltaV.Tag.Components;

/// <summary>
/// Overrides tags, either removing or adding them on the fly, without needing to fuck with inheritance
/// </summary>
[RegisterComponent]
public sealed partial class TagOverrideComponent : Component
{
    [DataField(customTypeSerializer: typeof(PrototypeIdHashSetSerializer<TagPrototype>))]
    //[Access(typeof(TagSystem), Other = AccessPermissions.ReadExecute)] // FIXME Friends
    public HashSet<string>? RemoveTags = [];

    [DataField(customTypeSerializer: typeof(PrototypeIdHashSetSerializer<TagPrototype>))]
    //[Access(typeof(TagSystem), Other = AccessPermissions.ReadExecute)] // FIXME Friends
    public HashSet<string>? AddTags = [];
}
