using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared.Abilities.Psionics
{
    [RegisterComponent]
    public sealed partial class PyrokinesisPowerComponent : Component
    {
        public EntityTargetActionComponent? PyrokinesisPowerAction = null;
        [ValidatePrototypeId<EntityPrototype>]
        public const string PyrokinesisActionPrototype = "ActionPyrokinesis";
    }
}
