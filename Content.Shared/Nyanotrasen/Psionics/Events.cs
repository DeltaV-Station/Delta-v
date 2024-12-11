using Robust.Shared.Serialization;
using Content.Shared.DoAfter;

namespace Content.Shared.Psionics.Events
{
    [Serializable, NetSerializable]
    public sealed partial class PsionicRegenerationDoAfterEvent : DoAfterEvent
    {
        [DataField("startedAt", required: true)]
        public TimeSpan StartedAt;

        private PsionicRegenerationDoAfterEvent()
        {
        }

        public PsionicRegenerationDoAfterEvent(TimeSpan startedAt)
        {
            StartedAt = startedAt;
        }

        public override DoAfterEvent Clone() => this;
    }

    // DeltaV Precognition
    [Serializable, NetSerializable]
    public sealed partial class PrecognitionDoAfterEvent : SimpleDoAfterEvent
    {
        [DataField("startedAt", required: true)]
        public TimeSpan StartedAt;

        private PrecognitionDoAfterEvent()
        {
        }

        public PrecognitionDoAfterEvent(TimeSpan startedAt)
        {
            StartedAt = startedAt;
        }

        public override DoAfterEvent Clone() => this;
    }
    // DeltaV End Precognition

    [Serializable, NetSerializable]
    public sealed partial class GlimmerWispDrainDoAfterEvent : SimpleDoAfterEvent
    {
    }
}
