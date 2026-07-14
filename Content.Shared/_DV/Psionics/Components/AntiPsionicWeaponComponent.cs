using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Psionics.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class AntiPsionicWeaponComponent : Component
{
    /// <summary>
    /// The DamageModifiers for each DamageType.
    /// </summary>
    [DataField(required: true)]
    public DamageModifierSet Modifiers = default!;

    /// <summary>
    /// The additional stamina damage dealt by anti-psionic weaponry.
    /// </summary>
    [DataField]
    public float StaminaDamageMultiplier = 1f;

    /// <summary>
    /// The chance to disable the target's psionic abilities on hit.
    /// </summary>
    [DataField]
    public float DisableChance = 0.3f;

    /// <summary>
    /// Punish the user when used against a non-psionic target.
    /// </summary>
    [DataField]
    public bool Punish;

    /// <summary>
    /// The chance for the weapon to punish the user when used against a non-psionic target.
    /// </summary>
    [DataField]
    public float PunishChance = 0.5f;

    /// <summary>
    /// The sound created when hitting a psionic user with the weapon or being punished.
    /// </summary>
    [DataField]
    public SoundSpecifier? HitSound = new SoundPathSpecifier("/Audio/Effects/lightburn.ogg");
}
