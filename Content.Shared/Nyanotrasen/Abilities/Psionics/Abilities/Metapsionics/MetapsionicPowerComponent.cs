using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared.Abilities.Psionics
{
    [RegisterComponent]
    public sealed partial class MetapsionicPowerComponent : Component
    {
        [DataField("range")]
        public float Range = 5f;

        public InstantActionComponent? MetapsionicPowerAction = null;
        public const string MetapsionicActionPrototype = "ActionMetapsionic";
    }
}
