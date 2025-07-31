using Content.Server._DV.StationEvents.Events;

namespace Content.Server._DV.StationEvents.Components;

[RegisterComponent, Access(typeof(PsionicNosebleedRule))]
public sealed partial class PsionicNosebleedRuleComponent : Component
{
    [DataField]
    public float BleedAmount = 2.5f;
}
