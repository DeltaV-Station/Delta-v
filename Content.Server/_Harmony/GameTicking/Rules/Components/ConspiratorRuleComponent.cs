using Content.Shared.Random;
using Robust.Shared.Prototypes;

namespace Content.Server._Harmony.GameTicking.Rules.Components;

/// <summary>
/// Game rule for conspirators. Handles their shared objective.
/// </summary>
[RegisterComponent, Access(typeof(ConspiratorRuleSystem))]
public sealed partial class ConspiratorRuleComponent : Component
{
    [DataField]
    public EntProtoId? Objective = null;
    
    [DataField]
    public ProtoId<WeightedRandomPrototype> ObjectiveGroup = "ConspiratorObjectiveGroup";
}
