using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Component for revealing cosmic cultists to the crew.
/// </summary>
[NetworkedComponent, RegisterComponent]
public sealed class RogueAscendedInfectionComponent : Component
{
    [DataField]
    public bool HadMoods;

    [DataField]
    public SpriteSpecifier Sprite = new SpriteSpecifier.Rsi(new ResPath("/Textures/_DV/CosmicCult/Effects/ascendantinfection.rsi"), "vfx");
}

[Serializable, NetSerializable]
public enum AscendedInfectionKey
{
    Key,
}
