using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._DV.AkashicFold;

/// <summary>
/// Prototype for any ruin that spawns in the Akashic Fold.
/// </summary>
[Prototype()]
public sealed partial class AkashicRuinPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    /// Relative directory path to the given grid, i.e. `Maps/_DV/AkashicFold/Ruins/example.yml`
    /// </summary>
    [DataField(required: true)]
    public ResPath MapPath;

    /// <summary>
    /// Keep the grid's roof? Enable for ruins that are mostly indoors. Mainly matters for planetary lighting.
    /// </summary>
    [DataField]
    public bool RoofEnabled;
}
