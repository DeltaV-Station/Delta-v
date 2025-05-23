using Content.Shared._DV.BloodDraining.EntitySystems;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.BloodDraining.Components;

/// <summary>
/// Marks that this entity is able to drain the blood of other beings
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedBloodDrainerSystem))]
public sealed partial class BloodDrainerComponent : Component
{
    /// <summary>
    /// How much to blood to drain on each attempt.
    /// </summary>
    [DataField]
    public float UnitsToDrain = 20f;

    /// <summary>
    /// How far away is this entity allowed to be in order to drain?
    /// </summary>
    [DataField]
    public float Distance = 1.5f;

    /// <summary>
    /// The time (in seconds) that it takes to drain an entity.
    /// </summary>
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(4);

    /// <summary>
    /// How much and what kind of damage, if any, to cause when draining from a victim.
    /// </summary>
    [DataField]
    public DamageSpecifier DamageOnDrain = new()
    {
        DamageDict = new()
        {
            { "Piercing", 1 }
        }
    };

    /// <summary>
    /// The sound to play when blood has been drained from a victim.
    /// </summary>
    [DataField]
    public SoundSpecifier DrainSound = new SoundPathSpecifier("/Audio/Items/drink.ogg");

    /// <summary>
    /// Whether the drainer is able to repeatedly drain blood from a victim.
    /// </summary>
    [DataField]
    public bool Repeatable = false;
}
