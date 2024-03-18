using Content.Shared.Random;

namespace Content.Server.Nyanotrasen.Cloning
{
    [RegisterComponent]
    public sealed partial class MetempsychoticMachineComponent : Component
    {
        /// <summary>
        /// Chance you will spawn as a humanoid instead of a non humanoid.
        /// </summary>
        [DataField("humanoidBaseChance")]
        public float HumanoidBaseChance = 0.75f;

        [ValidatePrototypeId<WeightedRandomPrototype>]
        [DataField("metempsychoticHumanoidPool")]
        public string MetempsychoticHumanoidPool = "MetempsychoticHumanoidPool";

        [ValidatePrototypeId<WeightedRandomPrototype>]
        [DataField("metempsychoticNonHumanoidPool")]
        public string MetempsychoticNonHumanoidPool = "MetempsychoticNonhumanoidPool";
    }
}
