using Content.Shared.Random;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Rules.Components;

/// <summary>
/// Taken from ThiefRuleComponent
/// Stores data for <see cref="RoundstartFugitiveRuleSystem"/>.
/// </summary>
[RegisterComponent, Access(typeof(RoundstartFugitiveRuleSystem))]
public sealed partial class RoundstartFugitiveRuleComponent : Component;
