using Content.Shared.Alert;
using Content.Shared.FixedPoint; // DeltaV
using Content.Shared.Ninja.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Ninja.Components;

/// <summary>
/// Component placed on a mob to make it a space ninja, able to use suit and glove powers.
/// Contains ids of all ninja equipment.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedSpaceNinjaSystem))]
public sealed partial class SpaceNinjaComponent : Component
{
    /// <summary>
    /// Currently worn suit
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Suit;

    /// <summary>
    /// Currently worn gloves, if enabled.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Gloves;

    /// <summary>
    /// Bound katana, set once picked up and never removed
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Katana;

    /// <summary>
    /// Objective to complete after calling in a threat.
    /// </summary>
    [DataField]
    public EntProtoId TerrorObjective = "TerrorObjective";

    /// <summary>
    /// Objective to complete after setting everyone to arrest.
    /// </summary>
    [DataField]
    public EntProtoId MassArrestObjective = "MassArrestObjective";

    /// <summary>
    /// Objective to complete after the spider charge detonates.
    /// </summary>
    [DataField]
    public EntProtoId SpiderChargeObjective = "SpiderChargeObjective";

    /// <summary>
    /// Alert to show for suit power.
    /// </summary>
    [DataField]
    public ProtoId<AlertPrototype> SuitPowerAlert = "SuitPower";

    /// <summary>
    /// DeltaV - The minimum damage to reveal the ninja on damage. Should be positive, since negative values are considered healing.
    /// Note that the ninja suit has 20% brute resist and punches deal 5 damage, so 4 should be enough to reveal.
    /// </summary>
    [DataField]
    public FixedPoint2 MinimumRevealDamage = 4;
}
