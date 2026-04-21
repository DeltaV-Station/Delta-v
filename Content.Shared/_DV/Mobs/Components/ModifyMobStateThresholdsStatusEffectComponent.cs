using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Mobs.Components;

/// <summary>
/// This component on a StatusEffect modifies the target's mob thresholds.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ModifyMobStateThresholdsStatusEffectComponent : Component
{
    /// <summary>
    /// The amount of required damage that will be added to the threshold of the mobstate.
    /// Negative numbers reduce required damage.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<MobState, FixedPoint2> Thresholds;
}
