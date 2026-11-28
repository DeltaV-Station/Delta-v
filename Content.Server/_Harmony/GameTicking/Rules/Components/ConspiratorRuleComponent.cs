using Content.Shared.Random;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._Harmony.GameTicking.Rules.Components;

/// <summary>
/// Game rule for conspirators. Handles their shared objective.
/// </summary>
[RegisterComponent, Access(typeof(ConspiratorRuleSystem))]
[AutoGenerateComponentPause]
public sealed partial class ConspiratorRuleComponent : Component
{
    [DataField]
    public EntProtoId? Objective = null;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? ConspiratorLeaderVoteTimer;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? ConspiratorObjectiveVoteTimer;
}
