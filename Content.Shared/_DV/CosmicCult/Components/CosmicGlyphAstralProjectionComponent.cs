using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.CosmicCult.Components;

[RegisterComponent]
public sealed class CosmicGlyphAstralProjectionComponent : Component
{
    /// <summary>
    /// The duration of the astral projection
    /// </summary>
    [DataField]
    public TimeSpan AstralDuration = TimeSpan.FromSeconds(12);

    [DataField]
    public DamageSpecifier ProjectionDamage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2> {
            { "Asphyxiation", 40 },
        },
    };

    [DataField]
    public EntProtoId SpawnProjection = "MobCosmicAstralProjection";
}
