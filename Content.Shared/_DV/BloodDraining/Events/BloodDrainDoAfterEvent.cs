using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.BloodDraining.Events;

/// <summary>
/// Simple do-after raised when a blood drainer attempts to start draining.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class BloodDrainDoAfterEvent : SimpleDoAfterEvent
{
}
