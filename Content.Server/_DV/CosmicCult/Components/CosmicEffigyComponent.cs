namespace Content.Server._DV.CosmicCult.Components;

[RegisterComponent]
public sealed partial class CosmicEffigyComponent : Component
{
    [DataField]
    public EntityUid? Colossus;

    [DataField]
    public float ColossusAttackRateMultiplier = 1.2f;

    [DataField]
    public float? ColossusAttackRateMax = 1.5f;

    [DataField]
    public float ColossusCorruptionSpeedMultiplier = 0.9f;

    [DataField]
    public bool ColossusHeal = true;
}
