using Content.Server.DeltaV.Objectives.Systems;

namespace Content.Server.DeltaV.Objectives.Components;

/// <summary>
///     Kidnap some number of heads. Use the NumberObjective to set the exact number
/// </summary>
[RegisterComponent, Access(typeof(KidnapHeadsConditionSystem))]
public sealed partial class KidnapHeadsConditionComponent: Component
{
}
