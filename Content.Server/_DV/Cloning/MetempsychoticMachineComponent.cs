using Content.Shared.Random;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Cloning;

[RegisterComponent]
public sealed partial class MetempsychoticMachineComponent : Component
{
    /// <summary>
    /// Base probability of remaining humanoid during cloning. Higher karma reduces this chance.
    /// </summary>
    [DataField]
    public float HumanoidBaseChance = 0.75f;

    /// <summary>
    /// Species prototypes pool to use for humanoids.
    /// </summary>
    [DataField]
    public ProtoId<WeightedRandomPrototype> MetempsychoticHumanoidPool = "MetempsychoticHumanoidPool";

    /// <summary>
    /// Entitiy prototypes pool to use for non-humanoids.
    /// </summary>
    [DataField]
    public ProtoId<WeightedRandomPrototype> MetempsychoticNonHumanoidPool = "MetempsychoticNonhumanoidPool";
}

