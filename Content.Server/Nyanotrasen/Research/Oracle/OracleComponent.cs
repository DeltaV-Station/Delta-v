using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Server.Research.Oracle;

[RegisterComponent]
public sealed partial class OracleComponent : Component
{
    public const string SolutionName = "fountain";

    [ViewVariables]
    [DataField("accumulator")]
    public float Accumulator;

    [ViewVariables]
    [DataField("resetTime")]
    public TimeSpan ResetTime = TimeSpan.FromMinutes(10);

    [DataField("barkAccumulator")]
    public float BarkAccumulator;

    [DataField("barkTime")]
    public TimeSpan BarkTime = TimeSpan.FromMinutes(1);

    [ViewVariables(VVAccess.ReadWrite)]
    public EntityPrototype DesiredPrototype = default!;

    [ViewVariables(VVAccess.ReadWrite)]
    public EntityPrototype? LastDesiredPrototype = default!;

    [DataField("rewardReagents", customTypeSerializer: typeof(PrototypeIdListSerializer<ReagentPrototype>))]
    public IReadOnlyList<string> RewardReagents = new[]
    {
        "LotophagoiOil", "LotophagoiOil", "LotophagoiOil", "LotophagoiOil", "LotophagoiOil", "Wine", "Blood", "Ichor"
    };

    [DataField("demandMessages")]
    public IReadOnlyList<string> DemandMessages = new[]
    {
        "oracle-demand-1",
        "oracle-demand-2",
        "oracle-demand-3",
        "oracle-demand-4",
        "oracle-demand-5",
        "oracle-demand-6",
        "oracle-demand-7",
        "oracle-demand-8",
        "oracle-demand-9",
        "oracle-demand-10",
        "oracle-demand-11",
        "oracle-demand-12"
    };

    [DataField("rejectMessages")]
    public IReadOnlyList<string> RejectMessages = new[]
    {
        "ἄγνοια",
        "υλικό",
        "ἀγνωσία",
        "γήινος",
        "σάκλας"
    };

    [DataField("blacklistedPrototypes")]
    [ViewVariables(VVAccess.ReadOnly)]
    public IReadOnlyList<string> BlacklistedPrototypes = new[]
    {
        "Drone",
        "QSI",
        "HandTeleporter",
        "BluespaceBeaker",
        "ClothingBackpackHolding",
        "ClothingBackpackSatchelHolding",
        "ClothingBackpackDuffelHolding",
        "TrashBagOfHolding",
        "BluespaceCrystal",
        "InsulativeHeadcage",
        "CrystalNormality",
    };
}
