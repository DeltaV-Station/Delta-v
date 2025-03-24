using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Component for displaying Vacuous Imposition's visuals on a player.
/// </summary>
[NetworkedComponent, RegisterComponent]
public sealed partial class CosmicImposingComponent : Component
{
    [DataField]
    public ResPath RsiPath = new("/Textures/_DV/CosmicCult/Effects/ability_imposition_overlay.rsi");
    public readonly string States = "vfx";
}

[Serializable, NetSerializable]
public enum CosmicImposingKey
{
    Key
}
