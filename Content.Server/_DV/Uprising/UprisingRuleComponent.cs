using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._DV.Uprising;

[RegisterComponent, Access(typeof(UprisingRuleSystem)), AutoGenerateComponentPause]
public sealed partial class UprisingRuleComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? NukeAnnouncementAt;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? NukeTimeAt;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? FirstWarningAt;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? ImpendingWarningAt;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? FinalWarningAt;

    [DataField]
    public TimeSpan NukeAnnouncementDelay = TimeSpan.FromMinutes(30);

    [DataField]
    public TimeSpan NukeTimeDelay = TimeSpan.FromMinutes(90);

    [DataField]
    public TimeSpan FirstWarning = TimeSpan.FromMinutes(30);

    [DataField]
    public TimeSpan ImpendingWarning = TimeSpan.FromMinutes(15);

    [DataField]
    public TimeSpan FinalWarning = TimeSpan.FromMinutes(5);

    [DataField]
    public float NukeDuration = 60f;
}
