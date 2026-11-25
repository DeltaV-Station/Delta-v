using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Psionics.Events;

[Serializable, NetSerializable]
public sealed partial class GlimmerWispDrainDoAfterEvent : SimpleDoAfterEvent;
