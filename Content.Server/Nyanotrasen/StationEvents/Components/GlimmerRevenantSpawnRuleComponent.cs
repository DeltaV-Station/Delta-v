using Content.Server.StationEvents.Events;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(GlimmerRevenantRule))]
public sealed partial class GlimmerRevenantRuleComponent : Component
{
    [DataField("prototype")]
    public string RevenantPrototype = "MobRevenant";
}
