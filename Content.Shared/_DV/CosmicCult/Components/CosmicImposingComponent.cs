using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Utility;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Component for displaying Vacuous Imposition's visuals on a player.
/// </summary>
[NetworkedComponent, RegisterComponent]
[AutoGenerateComponentPause]
public sealed class CosmicImposingComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan Expiry;

    [DataField]
    public SpriteSpecifier Sprite = new SpriteSpecifier.Rsi(new ResPath("/Textures/_DV/CosmicCult/Effects/ability_imposition_overlay.rsi"), "vfx");
}

[Serializable, NetSerializable]
public enum CosmicImposingKey
{
    Key,
}
