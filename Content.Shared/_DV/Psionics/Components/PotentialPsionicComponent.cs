using Content.Shared.EntityTable;
using Content.Shared.EntityTable.EntitySelectors;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Psionics.Components;

/// <summary>
/// Entities with this component can become psionics at roundstart or via rerolling.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PotentialPsionicComponent : Component
{
    /// <summary>
    /// The Base Chance for all potential psionics to become one at roundstart and gain a power.
    /// </summary>
    [DataField]
    public float BaseChance = 0.15f;

    /// <summary>
    /// Additive bonus for <see cref="BaseChance"/> from your role.
    /// </summary>
    /// <example>Command roles and the Chaplain get an additive bonus to being psionic.</example>
    [DataField]
    public float JobBonusChance;

    /// <summary>
    /// Additive bonus for <see cref="BaseChance"/> from your species.
    /// </summary>
    /// <example>Noöspheric attuned species like Kitsunes get an additive bonus.</example>
    [DataField]
    public float SpeciesBonusChance;

    /// <summary>
    /// Whether you've already attempted to roll a new power via other means.
    /// </summary>
    /// <example>Lotophagoi Oil will attempt to roll a new power for the consumer if this is false.
    /// It'll then be set true.</example>
    [DataField]
    public bool Rolled;

    /// <summary>
    /// The available psionics to roll.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityTableSelector? AvailablePsionics;
}
