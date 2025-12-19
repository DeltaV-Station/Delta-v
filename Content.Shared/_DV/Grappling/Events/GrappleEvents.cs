using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Grappling.Events;

/// <summary>
/// Raised when a player attempts to wriggle from a grapple.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class GrappledEscapeDoAfter : SimpleDoAfterEvent;
