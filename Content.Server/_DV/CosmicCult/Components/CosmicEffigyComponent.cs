using Content.Shared.FixedPoint;

namespace Content.Server._DV.CosmicCult.Components;

[RegisterComponent]
public sealed partial class CosmicEffigyComponent : Component
{
    /// <summary>
    /// The colossus that placed this effigy.
    /// </summary>
    [DataField]
    public EntityUid? Colossus;
}
