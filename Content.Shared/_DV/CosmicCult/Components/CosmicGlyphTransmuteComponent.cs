using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.CosmicCult.Components;

[RegisterComponent]
public sealed class CosmicGlyphTransmuteComponent : Component
{
    /// <summary>
    ///     A pool of entities that we pick from when transmuting.
    /// </summary>
    [DataField(required: true)]
    public HashSet<EntProtoId> Transmutations;

    /// <summary>
    ///     The search range for finding transmutation targets.
    /// </summary>
    [DataField] public float TransmuteRange = 0.5f;

    /// <summary>
    ///     Permissible entities for the transmutation
    /// </summary>
    [DataField(required: true)]
    public EntityWhitelist Whitelist;
}
