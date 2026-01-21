using Robust.Shared.Audio;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._DV.Abilities.Psionics; // why is this in the Shared namespace but the Server folder?

[RegisterComponent, AutoGenerateComponentPause]
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
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextAnnoy = TimeSpan.FromSeconds(5);
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextSpark = TimeSpan.MaxValue;
    [DataField]
    public bool Warned = false;
}
