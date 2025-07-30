using Content.Server._DV.StationEvents.Events;

namespace Content.Server._DV.StationEvents.Components;

[RegisterComponent, Access(typeof(RandomAnimationRule))]
public sealed partial class RandomAnimationRuleComponent : Component
{
    [DataField]
    public int MinAnimates = 5;

    [DataField]
    public int MaxAnimates = 7;

    [DataField]
    public float AnimationTime = 10f;
}
