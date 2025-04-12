using Content.Shared.Atmos;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.CosmicCult.Components;

[RegisterComponent]
public sealed partial class CosmicSpireComponent : Component
{
    [DataField]
    public HashSet<Gas> DrainGases =
    [
        Gas.Oxygen,
        Gas.Nitrogen,
        Gas.CarbonDioxide,
        Gas.WaterVapor,
        Gas.Ammonia,
        Gas.NitrousOxide,
    ];

    [DataField]
    public float DrainRate = 550;

    [DataField]
    public float DrainThreshHold = 2500;

    [DataField]
    public bool Enabled;

    [DataField]
    public EntProtoId EntropyMote = "MaterialCosmicCultEntropy1";

    [DataField]
    public EntProtoId SpawnVFX = "CosmicGenericVFX";

    [DataField]
    public GasMixture Storage = new();
}

[Serializable, NetSerializable]
public enum SpireVisuals : byte
{
    Status,
}

[Serializable, NetSerializable]
public enum SpireStatus : byte
{
    Off,
    On,
}
