using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Psionics.Events.PowerDoAfterEvents;

[Serializable, NetSerializable]
public sealed partial class PsionicEruptionDoAfterEvent : SimpleDoAfterEvent;
