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
    /// The variance that will be added or subtracted from the initial value.
    /// </summary>
    [DataField]
    public float Variance = 0.5f;

    /// <summary>
    /// The sound that occurs when being dispelled.
    /// </summary>
    [DataField]
    public SoundSpecifier DispelSound = new SoundPathSpecifier("/Audio/Effects/lightburn.ogg");
}

