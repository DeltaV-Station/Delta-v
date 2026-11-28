using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.Weapons.Melee;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class WrathMeleeComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Wrath = 0;

    [DataField(required: true)]
    public int WrathPerHit;

    [DataField(required: true)]
    public int WrathFallAmount;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan NextWrathFall = TimeSpan.Zero;

    [DataField, AutoNetworkedField]
    public TimeSpan WrathFallRate = TimeSpan.FromSeconds(1);

    [DataField(required: true)]
    public List<WrathMeleeTier> Tiers;

    [DataField, AutoNetworkedField]
    public EntityUid? Holder;

    [Serializable, NetSerializable]
    public enum WrathMeleeVisuals : byte
    {
        State
    }

    [Serializable, NetSerializable]
    public enum WrathMeleeTierVisual : byte
    {
        TierOne,
        TierTwo,
        TierThree
    }
}
