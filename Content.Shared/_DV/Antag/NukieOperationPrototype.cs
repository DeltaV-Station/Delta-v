using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Antag;

/// <summary>
///     This is for nukie operations. E.g nuke the station or kidnap x number of heads.
/// </summary>
[Prototype]
public sealed partial class NukieOperationPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    [DataField(required: true)]
    public List<EntProtoId> OperationObjectives = new();

    [DataField]
    public LocId? NukeCodePaperOverride;
}
