using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.Psionics.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HystericalStrengthStatusEffectComponent : Component
{
    /// <summary>
    /// The damage dealt to entities that have this effect.
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier Damage;

    /// <summary>
    /// How long each damage tick is delayed by.
    /// </summary>
    [DataField]
    public TimeSpan DamageDelay = TimeSpan.FromSeconds(1);

    /// <summary>
    /// The next tick for the damage to be applied.
    /// </summary>
    [DataField(customTypeSerializer:typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan NextTick;

    /// <summary>
    /// The stun duration when the psionic gets dispelled while actively using that ability.
    /// </summary>
    [DataField]
    public TimeSpan StunDurationOnDispel = TimeSpan.FromSeconds(6);

    /// <summary>
    /// How much glimmer it generates each damage tick.
    /// </summary>
    [DataField]
    public int PassiveGlimmerGeneration = 1;
}
