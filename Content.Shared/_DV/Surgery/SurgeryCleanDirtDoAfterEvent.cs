using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Surgery;

[Serializable, NetSerializable]
public sealed partial class SurgeryCleanDirtDoAfterEvent : SimpleDoAfterEvent;
