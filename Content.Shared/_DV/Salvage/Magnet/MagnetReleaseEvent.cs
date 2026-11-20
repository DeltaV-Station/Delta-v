using Robust.Shared.Serialization;

namespace Content.Shared._DV.Salvage.Magnet;

/// <summary>
/// Claim an offer from the magnet UI.
/// </summary>
[Serializable, NetSerializable]
public sealed class MagnetReleaseEvent : BoundUserInterfaceMessage { }
