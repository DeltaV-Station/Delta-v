using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Weapons.Melee;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class CursedDaggerComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Adrenaline = 0;

    [DataField(required: true)]
    public int AdrenalinePerHit;

    [DataField(required: true)]
    public int AdrenalineFallAmount;

    [DataField(required: true)]
    public int AdrenalineTierTwo;

    [DataField(required: true)]
    public int AdrenalineTierThree;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan NextAdrenalineFall = TimeSpan.Zero;

    [DataField, AutoNetworkedField]
    public TimeSpan AdrenalineFallRate = TimeSpan.FromSeconds(1);

    [DataField(required: true)]
    public DamageSpecifier TierOneDamage;

    [DataField(required: true)]
    public DamageSpecifier TierTwoDamage;

    [DataField(required: true)]
    public DamageSpecifier TierThreeDamage;

    [DataField(required: true)]
    public float SpeedBoost;

    [DataField, AutoNetworkedField]
    public EntityUid? Holder;

    [DataField(required: true)]
    public float TierTwoReflect;

    [DataField(required: true)]
    public float TierThreeReflect;
}