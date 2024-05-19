using Content.Shared.Roles;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Stray.Wizard.Components;

[RegisterComponent, Access(typeof(WizardRuleSystem))]
public sealed partial class WizardRuleComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan? NextRoundEndCheck;

    [DataField]
    public TimeSpan EndCheckDelay = TimeSpan.FromSeconds(30);

    [DataField]
    public float ZombieShuttleCallPercentage = 0.7f;

[DataDefinition, Serializable]
public sealed partial class NukeopSpawnPreset
{

    [DataField]
    public ProtoId<AntagPrototype> AntagRoleProto = "Wizard";
}
}
