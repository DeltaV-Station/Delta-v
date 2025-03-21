using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Component for revealing cosmic cultists to the crew.
/// </summary>
[NetworkedComponent, RegisterComponent]
public sealed partial class RogueAscendedAuraComponent : Component
{
    public ResPath RsiPath = new("/Textures/_DV/CosmicCult/Effects/ascendantaura.rsi");

    public readonly string States = "vfx";
}

[Serializable, NetSerializable]
public enum AscendedAuraKey
{
    Key
}
