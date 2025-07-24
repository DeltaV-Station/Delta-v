using Content.Shared.DoAfter;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Psionics.Events;

[Serializable, NetSerializable]
public sealed partial class PsionicRegenerationDoAfterEvent : SimpleDoAfterEvent
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan StartedAt;

    private PsionicRegenerationDoAfterEvent()
    {
    }

    public PsionicRegenerationDoAfterEvent(TimeSpan startedAt)
    {
        StartedAt = startedAt;
    }
}

[Serializable, NetSerializable]
public sealed partial class PsionicEruptionDoAfterEvent : SimpleDoAfterEvent
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan StartedAt;

    private PsionicEruptionDoAfterEvent()
    {
    }

    public PsionicEruptionDoAfterEvent(TimeSpan startedAt)
    {
        StartedAt = startedAt;
    }
}
