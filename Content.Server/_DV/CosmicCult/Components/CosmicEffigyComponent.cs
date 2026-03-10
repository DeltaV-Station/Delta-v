using Content.Shared.FixedPoint;

namespace Content.Server._DV.CosmicCult.Components;

[RegisterComponent]
public sealed partial class CosmicEffigyComponent : Component
{
    [DataField]
    public EntityUid? Colossus;

    [DataField]
    public float ColossusAttackRateMultiplier = 1.1f;

    [DataField]
    public float ColossusCorruptionSpeedMultiplier = 0.9f;

    [DataField]
    public FixedPoint2 ColossusBonusDamage = 10;

    [DataField]
    public bool ColossusHeal = true;
}
