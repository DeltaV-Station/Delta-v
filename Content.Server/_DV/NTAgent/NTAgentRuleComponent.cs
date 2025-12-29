using Content.Shared.Random;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.GameObjects;

namespace Content.Server._DV.NTAgent;

[RegisterComponent, Access(typeof(NTAgentRuleSystem))]
public sealed partial class NTAgentRuleComponent : Component;
