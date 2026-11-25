using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Psionics.Components;

/// <summary>
/// Takes damage when dispelled.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DamageOnDispelComponent : Component
{
    /// <summary>
    /// The damage dealt to them on being dispelled.
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier Damage = default!;

    /// <summary>
    /// The variance that will be multiplied with the initial value.
    /// A variance of 0.5 will lead to the damage dealing either minimum half the damage, or 1.5x the damage maximum.
    /// </summary>
    [DataField]
    public float Variance = 0.5f;

    /// <summary>
    /// The sound that occurs when being dispelled.
    /// </summary>
    [DataField]
    public SoundSpecifier DispelSound = new SoundPathSpecifier("/Audio/Effects/lightburn.ogg");
}
