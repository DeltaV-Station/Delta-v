using Content.Shared.Damage;
using Content.Shared._Shitmed.Targeting; // Shitmed
using Robust.Shared.GameStates;

namespace Content.Shared.Trigger.Components.Effects;

/// <summary>
/// Will damage an entity when triggered.
/// If TargetUser is true it the user will take damage instead.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DamageOnTriggerComponent : BaseXOnTriggerComponent
{
    /// <summary>
    /// Should the damage ignore resistances?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IgnoreResistances;

    /// <summary>
    /// The base damage amount that is dealt.
    /// May be further modified by <see cref="Systems.BeforeDamageOnTriggerEvent"/> subscriptions.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public DamageSpecifier Damage = default!;

    /// <summary>
    /// Shitmed Change: Lets mousetraps, etc. target the feet.
    /// </summary>
    [DataField]
    public TargetBodyPart? TargetPart = TargetBodyPart.Feet;
}
