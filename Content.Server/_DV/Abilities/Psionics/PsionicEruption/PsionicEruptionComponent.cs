using Robust.Shared.Audio;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._DV.Abilities.Psionics;

[RegisterComponent]
public sealed partial class PsionicEruptionPowerComponent : Component
{
    [DataField]
    public DoAfterId? DoAfter;
    [DataField]
    public EntProtoId EruptionActionId = "ActionEruption";
    [DataField]
    public EntityUid? EruptionActionEntity;
    [DataField]
    public SoundSpecifier SoundUse = new SoundPathSpecifier("/Audio/Nyanotrasen/Psionics/heartbeat_fast.ogg");
    [DataField]
    public SoundSpecifier SoundDetonate = new SoundPathSpecifier("/Audio/Nyanotrasen/Psionics/eruption.ogg");
    [DataField]
    public TimeSpan NextAnnoy = TimeSpan.FromSeconds(5);
    [DataField]
    public TimeSpan NextSpark = TimeSpan.MaxValue;
    [DataField]
    public bool Warned = false;
}
