using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.BloodDraining.Events;

[Serializable, NetSerializable]
public sealed partial class BloodDrainDoAfterEvent : SimpleDoAfterEvent
{
}
