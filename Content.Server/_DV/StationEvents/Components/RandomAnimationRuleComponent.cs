using Content.Server._DV.StationEvents.Events;

namespace Content.Server._DV.StationEvents.Components;

[RegisterComponent, Access(typeof(RandomAnimationRule))]
public sealed partial class RandomAnimationRuleComponent : Component
{
    [DataField]
    public int MinAnimates = 7;

    [DataField]
    public int MaxAnimates = 10;

    [DataField]
    public float MinTime = 7f;

    [DataField]
    public float MaxTime = 15f;
}
