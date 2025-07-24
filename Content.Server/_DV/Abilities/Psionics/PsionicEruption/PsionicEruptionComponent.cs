using Robust.Shared.Audio;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._DV.Abilities.Psionics;

[RegisterComponent]
public sealed partial class PsionicEruptionPowerComponent : Component
{
    [DataField("doAfter")]
    public DoAfterId? DoAfter;
    public InstantActionComponent? PsionicEruptionAction = null;
    [DataField("eruptionActionId",
    customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? EruptionActionId = "ActionEruption";

    [DataField("eruptionActionEntity")]
    public EntityUid? EruptionActionEntity;

    [DataField("soundUse")]

    public SoundSpecifier SoundUse = new SoundPathSpecifier("/Audio/Nyanotrasen/Psionics/heartbeat_fast.ogg");
    [DataField("soundDetonate")]
    public SoundSpecifier SoundDetonate = new SoundPathSpecifier("/Audio/Nyanotrasen/Psionics/eruption.ogg");

    [DataField("nextAnnoy")]
    public TimeSpan NextAnnoy = TimeSpan.FromSeconds(5);

    [DataField("warned")]
    public bool Warned = false;
}
