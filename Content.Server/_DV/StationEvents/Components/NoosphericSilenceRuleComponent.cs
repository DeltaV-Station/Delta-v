using Content.Server._DV.StationEvents.GameRules;

namespace Content.Server._DV.StationEvents.Components;

[RegisterComponent, Access(typeof(NoosphericSilenceRule))]
public sealed partial class NoosphericSilenceRuleComponent : Component
{
    [DataField]
    public TimeSpan MinDuration = TimeSpan.FromSeconds(20);

    [DataField]
    public TimeSpan MaxDuration = TimeSpan.FromSeconds(80);
}
