using Content.Server.StationEvents.Events;
using Robust.Shared.Prototypes;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(GlimmerMobRule))]
public sealed partial class GlimmerMobRuleComponent : Component
{
    [DataField(required: true)]
    public EntProtoId MobPrototype = string.Empty;
}
