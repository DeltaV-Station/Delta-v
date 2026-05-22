using Robust.Shared.GameStates;

namespace Content.Shared._DV.Whitelist.Holoprojectors;

/// <summary>
/// Marker component for holopeels, used for reclaiming charges of the projector.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HolopeelComponent : Component;
