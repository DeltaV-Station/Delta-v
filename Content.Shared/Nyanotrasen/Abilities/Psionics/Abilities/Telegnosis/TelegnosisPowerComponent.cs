using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared.Abilities.Psionics
{
    [RegisterComponent]
    public sealed partial class TelegnosisPowerComponent : Component
    {
        [DataField("prototype")]
        public string Prototype = "MobObserverTelegnostic";
        public InstantActionComponent? TelegnosisPowerAction = null;
        [ValidatePrototypeId<EntityPrototype>]
        public const string TelegnosisActionPrototype = "ActionTelegnosis";
    }
}