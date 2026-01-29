using Content.Server._DV.StationEvents.Events;

namespace Content.Server._DV.StationEvents.Components;

[RegisterComponent, Access(typeof(ThavenMoodUpset))]
public sealed partial class ThavenMoodUpsetRuleComponent : Component
{
    [DataField]
    public bool AddWildcardMood = false;

    [DataField]
    public bool NewSharedMoods = false;

    [DataField]
    public bool RefreshPersonalMoods = false;
}
