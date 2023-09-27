using Content.Shared.Actions;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Abilities.Psionics
{
    [RegisterComponent]
    public sealed partial class MassSleepPowerComponent : Component
    {
        public WorldTargetActionComponent? MassSleepPowerAction = null;

        public float Radius = 1.25f;

        /// <summary>
        /// The action for enabling and disabling mass sleep's trigger.
        /// </summary>
        [DataField("toggleActionEntity")] public EntityUid? ToggleActionEntity;

        [ValidatePrototypeId<EntityPrototype>]
        public const string MassSleepActionPrototype = "ActionMassSleep";
    }
}
