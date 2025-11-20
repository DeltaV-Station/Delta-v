using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Body.Events;

[Serializable, NetSerializable]
public sealed partial class CPRFinishedEvent : SimpleDoAfterEvent;
