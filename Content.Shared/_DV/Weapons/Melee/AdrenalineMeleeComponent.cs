using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.Weapons.Melee;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class AdrenalineMeleeComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Adrenaline = 0;

    [DataField(required: true)]
    public int AdrenalinePerHit;

    [DataField(required: true)]
    public int AdrenalineFallAmount;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan NextAdrenalineFall = TimeSpan.Zero;

    [DataField, AutoNetworkedField]
    public TimeSpan AdrenalineFallRate = TimeSpan.FromSeconds(1);

    [DataField(required: true)]
    public List<AdrenalineMeleeTier> Tiers;

    [DataField, AutoNetworkedField]
    public EntityUid? Holder;

    [Serializable, NetSerializable]
    public enum AdrenalineMeleeVisuals : byte
    {
        State
    }

    [Serializable, NetSerializable]
    public enum AdrenalineMeleeTierVisual : byte
    {
        TierOne,
        TierTwo,
        TierThree
    }
}
