using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared.Abilities.Psionics
{
    [RegisterComponent]
    public sealed partial class MindSwapPowerComponent : Component
    {
        public EntityTargetActionComponent? MindSwapPowerAction = null;

        [ValidatePrototypeId<EntityPrototype>]
        public const string MindSwapActionPrototype = "ActionMindSwap";
        public InstantActionComponent? MindSwapReturnPowerAction = null;

        [ValidatePrototypeId<EntityPrototype>]
        public const string MindSwapReturnActionPrototype = "ActionMindSwapReturn";
    }
}
