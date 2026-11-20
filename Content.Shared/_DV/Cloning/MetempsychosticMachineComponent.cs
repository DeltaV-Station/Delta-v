using Content.Shared.Random;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Cloning;

[RegisterComponent]
public sealed partial class MetempsychoticMachineComponent : Component
{
    /// <summary>
    /// The Base chance you will spawn as a human in stead of a non-humanoid
    /// </summary>
    [DataField("humanoidBaseChance")]
    public float HumanoidBaseChance = 0.75f;

    public static readonly ProtoId<WeightedRandomPrototype> MetempsychoticHumanoidPool = "MetempsychoticHumanoidPool";
    public static readonly ProtoId<WeightedRandomPrototype> MetempsychoticNonHumanoidPool = "MetempsychoticNonHumanoidPool";
}
