using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared.Abilities.Psionics
{
    [RegisterComponent]
    public sealed partial class NoosphericZapPowerComponent : Component
    {
        public EntityTargetActionComponent? NoosphericZapPowerAction = null;
        [ValidatePrototypeId<EntityPrototype>]
        public const string NoosphericZapActionPrototype = "ActionNoosphericZap";
    }
}
