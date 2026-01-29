using Content.Shared.Storage;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Rules.VariationPass.Components;

/// <summary>
/// This is used for spawning entities randomly dotted around the station in a variation pass.
/// </summary>
[RegisterComponent]
public sealed partial class EntitySpawnVariationPassComponent : Component
{
    /// <summary>
    ///     Number of tiles before we spawn one entity on average.
    /// </summary>
    [DataField]
    public float TilesPerEntityAverage = 50f;

    [DataField]
    public float TilesPerEntityStdDev = 7f;

    /// <summary>
    ///     Spawn entries for each chosen location.
    /// </summary>
    [DataField(required: true)]
    public List<EntitySpawnEntry> Entities = default!;

    /// <summary>
    ///     Delta-V: Skip spawning an entity if another entity with any of these tags exists on the tile.
    /// </summary>
    [DataField]
    public ProtoId<TagPrototype>[]? TagsBlacklist;
}
