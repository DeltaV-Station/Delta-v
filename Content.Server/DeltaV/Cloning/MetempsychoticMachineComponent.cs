using Content.Shared.Random;
using Robust.Shared.Prototypes;

namespace Content.Server.DeltaV.Cloning;

[RegisterComponent]
public sealed partial class MetempsychoticMachineComponent : Component
{
    /// <summary>
    ///     Chance you will spawn as a humanoid instead of a non humanoid.
    /// </summary>
    [DataField]
    public float HumanoidBaseChance = 0.75f;

    [DataField]
    public ProtoId<WeightedRandomPrototype> MetempsychoticHumanoidPool = "MetempsychoticHumanoidPool";

    [DataField]
    public ProtoId<WeightedRandomPrototype> MetempsychoticNonHumanoidPool = "MetempsychoticNonhumanoidPool";
}

