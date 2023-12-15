using Content.Server.StationEvents.Events;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(GlimmerMobRule))]
public sealed partial class GlimmerMobRuleComponent : Component
{
}
[Dependency] private readonly IRobustRandom _robustRandom = default!;
[Dependency] private readonly GlimmerSystem _glimmerSystem = default!;

[DataField(required: true)]
