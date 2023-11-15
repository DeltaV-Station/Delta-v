using Content.Client.Guidebook;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;
using Robust.Shared.Utility;

namespace Content.Client.DeltaV.TabbedRules;

[Virtual]
public class TabbedEntry
{
    [DataField("text", required: true)]
    public ResPath Text = default!;

    [IdDataField]
    public string Id = string.Empty;

    [DataField("container", required: true)]
    public string Container = string.Empty;

    [DataField("order")]
    public int Order;

    [DataField("name", required: true)]
    public string Name = default!;

    [DataField("children", customTypeSerializer:typeof(PrototypeIdListSerializer<GuideEntryPrototype>))]
    public List<string> Children = new();
}

[Prototype("tabbedEntry")]
public sealed class TabbedEntryPrototype : TabbedEntry, IPrototype
{
    public string ID => Id;
}
