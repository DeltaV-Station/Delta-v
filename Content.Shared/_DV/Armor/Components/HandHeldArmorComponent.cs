using Content.Shared._DV.Armor.Systems;
using Content.Shared.Damage;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Armor.Components;

/// <summary>
/// Used for armor that can be held in your hands.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(HandHeldArmorSystem))]
public sealed partial class HandHeldArmorComponent : Component
{
    /// <summary>
    /// The Entity holding the handheld armor.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Holder;

    /// <summary>
    /// The damage reduction
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public DamageModifierSet Modifiers;

    /// <summary>
    /// A multiplier applied to the calculated point value
    /// to determine the monetary value of the armor
    /// </summary>
    [DataField, AutoNetworkedField]
    public float PriceMultiplier = 1;

    /// <summary>
    /// If true, you can examine the armor to see the protection. If false, the verb won't appear.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ShowArmorOnExamine = true;

    /// <summary>
    /// DeltaV - Gets the effective stamina melee damage coefficient, based on the armor's blunt protection.
    /// </summary>
    [ViewVariables]
    public float StaminaMeleeDamageCoefficient => Modifiers.Coefficients.GetValueOrDefault("Blunt", 1.0f);

    /// <summary>
    /// The required components for the held armor to be active while held.
    /// </summary>
    /// <example>A Bible only protects the holder if they have the BibleUserComponent.</example>
    /// <remarks>No whitelist check when null.</remarks>
    [DataField, AutoNetworkedField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// The Loc string for the message shown in the armor examination if the user fails the whitelist requirements.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? WhitelistFailMessage;

    /// <summary>
    /// Components for the held armor to be inactive while held.
    /// </summary>
    /// <example>A clown with the clumsy cannot make use of a parrying dagger.</example>
    /// <remarks>No blacklist check when null.</remarks>
    [DataField, AutoNetworkedField]
    public EntityWhitelist? Blacklist;

    /// <summary>
    /// The Loc string for the message shown in the armor examination if the user fails the blacklist requirements.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? BlacklistFailMessage;
}
