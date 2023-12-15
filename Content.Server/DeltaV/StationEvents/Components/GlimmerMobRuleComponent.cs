using Content.Server.StationEvents.Events;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(GlimmerMobRule))]
public sealed partial class GlimmerMobRuleComponent : Component
{
    [DataField(required: true)]
    public EntProtoId MobPrototype = string.Empty;
}


[DataField(required: true)]
