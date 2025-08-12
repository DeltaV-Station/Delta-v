using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Component for Cosmic Cult's own radio jammer, because I'm not refactoring RadioJammerSystem.
/// </summary>
[RegisterComponent]
public sealed partial class CosmicJammerComponent : Component
{
    [DataField] public float Range = 25f;
    [DataField] public bool Active = false;
}

[Serializable, NetSerializable]
public enum JammerVisuals : byte
{
    Status,
}

[Serializable, NetSerializable]
public enum JammerStatus : byte
{
    Off,
    On,
}