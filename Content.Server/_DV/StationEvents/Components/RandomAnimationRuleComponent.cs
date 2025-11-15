using Content.Server._DV.StationEvents.Events;

namespace Content.Server._DV.StationEvents.Components;

[RegisterComponent, Access(typeof(RandomAnimationRule))]
public sealed partial class RandomAnimationRuleComponent : Component
{
    [DataField]
    public int MinAnimates = 40;

    [DataField]
    public int MaxAnimates = 60;

    [DataField]
    public float MinTime = 15f;

    [DataField]
    public float MaxTime = 30f;
}
