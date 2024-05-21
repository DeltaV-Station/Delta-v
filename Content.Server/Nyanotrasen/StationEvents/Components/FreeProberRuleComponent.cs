using Content.Server.Nyanotrasen.StationEvents.Events;
using Content.Server.StationEvents.Events;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(FreeProberRule))]
public sealed partial class FreeProberRuleComponent : Component
{
}
