using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared.Abilities.Psionics
{
    [RegisterComponent]
    public sealed partial class PsionicInvisibilityPowerComponent : Component
    {
        public InstantActionComponent? PsionicInvisibilityPowerAction = null;
        [ValidatePrototypeId<EntityPrototype>]
        public const string PsionicInvisibilityActionPrototype = "ActionMakePsionicallyInvisible";
    }
}
