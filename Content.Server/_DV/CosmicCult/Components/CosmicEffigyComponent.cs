using Content.Server._DV.CosmicCult.Abilities;
using Content.Server._DV.CosmicCult.EntitySystems;

namespace Content.Server._DV.CosmicCult.Components;

/// <summary>
/// Component for cosmic effigy anomalies, spawned by Cosmic Colossi.
/// </summary>
/// <seelso cref="CosmicEffigySystem"/>.
/// <seelso cref="CosmicColossusSystem"/>.
[RegisterComponent]
public sealed partial class CosmicEffigyComponent : Component
{
    /// <summary>
    /// The colossus that placed this effigy.
    /// </summary>
    [DataField]
    public EntityUid? Colossus;
}
