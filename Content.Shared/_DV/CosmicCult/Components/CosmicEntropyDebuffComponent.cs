using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Makes the target take damage over time.
/// Meant to be used in conjunction with statusEffectSystem.
/// </summary>
[RegisterComponent]
[AutoGenerateComponentPause]
public sealed class CosmicEntropyDebuffComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan CheckTimer;

    [DataField]
    public TimeSpan CheckWait = TimeSpan.FromSeconds(1);

    /// <summary>
    /// The debuff applied while the component is present.
    /// </summary>
    [DataField]
    public DamageSpecifier Degen = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Cold", 0.25},
            { "Asphyxiation", 1.25},
        },
    };

    /// <summary>
    /// The chance to recieve a message popup while under the effects of Entropic Degen.
    /// </summary>
    [DataField]
    public float PopupChance = 0.05f;
}
