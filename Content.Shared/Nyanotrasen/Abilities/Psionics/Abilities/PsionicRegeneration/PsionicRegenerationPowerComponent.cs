using Robust.Shared.Audio;
using Content.Shared.DoAfter;
using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared.Abilities.Psionics
{
    [RegisterComponent]
    public sealed partial class PsionicRegenerationPowerComponent : Component
    {
        [DataField("doAfter")]
        public DoAfterId? DoAfter;

        [DataField("essence")]
        public float EssenceAmount = 20;

        [DataField("useDelay")]
        public float UseDelay = 8f;

        [DataField("soundUse")]
        public SoundSpecifier SoundUse = new SoundPathSpecifier("/Audio/Nyanotrasen/heartbeat_fast.ogg");

        public InstantActionComponent? PsionicRegenerationPowerAction = null;
        [ValidatePrototypeId<EntityPrototype>]
        public const string PsionicRegenerationActionPrototype = "ActionPsionicRegeneration";
    }
}

