using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Component for revealing cosmic cultists to the crew.
/// </summary>
[NetworkedComponent, RegisterComponent]
public sealed class RogueAscendedAuraComponent : Component
{
    [DataField]
    public SpriteSpecifier Sprite = new SpriteSpecifier.Rsi(new ResPath("/Textures/_DV/CosmicCult/Effects/ascendantaura.rsi"), "vfx");
}

[Serializable, NetSerializable]
public enum AscendedAuraKey
{
    Key,
}
