using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Body.Components;

/// <summary>
/// Marks this entity as a feather, that probably has a custom color
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class FeatherComponent : Component
{
}

[Serializable, NetSerializable]
public enum FeatherVisuals : byte
{
    FeatherColor,
    BloodColor,
}
