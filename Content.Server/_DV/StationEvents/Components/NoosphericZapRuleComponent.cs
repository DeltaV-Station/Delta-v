using Content.Server._DV.StationEvents.GameRules;
using Content.Server.Nyanotrasen.StationEvents.Events;

namespace Content.Server._DV.StationEvents.Components;

[RegisterComponent, Access(typeof(NoosphericZapRule))]
public sealed partial class NoosphericZapRuleComponent : Component;
