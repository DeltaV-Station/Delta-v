using Robust.Shared.Prototypes;
namespace Content.Shared.Abilities.Psionics
{
    [RegisterComponent]
    public sealed partial class PsionicInvisibilityUsedComponent : Component
    {
        [ValidatePrototypeId<EntityPrototype>]
        public const string PsionicInvisibilityUsedActionPrototype = "ActionRemovePsionicInvisibility";
    }
}
