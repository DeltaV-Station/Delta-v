using Content.Shared.Chat.Prototypes;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.Body.Components;

/// <summary>
/// This is used for Avali feather preening functionality.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PreenableComponent : Component
{
    [DataField]
    public EntProtoId FeatherPrototype;

    [DataField]
    public HashSet<ProtoId<DamageGroupPrototype>>? ValidDamageGroups = new()
    {
        "Brute",
    };

    [DataField]
    public LocId SelfPreeningMessage = "preening-popup-self";

    [DataField]
    public LocId GettingPreenedMessage = "preening-popup-self-recipient";

    [DataField]
    public LocId PreeningOtherMessage = "preening-popup-other";

    [DataField]
    public LocId FeatherBloodiedNameString = "feather-bloody-name-modifier";

    [DataField]
    public LocId FeatherBloodiedDescString = "feather-bloody-desc";

    [DataField]
    public LocId PreeningVerbString = "preening-action-verb";

    [DataField]
    public LocId DroppedFeatherString = "preening-feather-dropped-injured";

    [DataField]
    public ProtoId<EmotePrototype> ScreamEmote = "Scream";

    /// <summary>
    /// The minimum amount of damage that must be taken from one attack to have a chance to shed a feather.
    /// </summary>
    [DataField]
    public FixedPoint2 ShedDamageThreshold = 9;

    /// <summary>
    /// The chance for a feather to be shed on hit, per point of damage taken.
    /// </summary>
    [DataField]
    public float ShedScalingChance = 0.0125f;

    [DataField]
    public DamageModifierSet? VulnerabilityModifier;

    [DataField]
    public float AimModifier = 0.75f;

    [DataField, AutoNetworkedField]
    public int MaximumFeathers = 3;

    [DataField, AutoNetworkedField]
    public int CurrentFeathers = 3;

    [DataField]
    public TimeSpan ReplenishDelay = TimeSpan.FromSeconds(180);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan? ReplenishTime;
}
