using Content.Shared._DV.Power.Components;

namespace Content.Client._DV.Power.Components;

[RegisterComponent]
[Virtual]
public sealed partial class BatteryComponent : SharedBatteryComponent
{
    /// <summary>
    /// Current charge of the battery in joules (ie. watt seconds)
    /// </summary>
    [DataField("startingCharge")]
    public override float CurrentCharge { get; set; }
}
