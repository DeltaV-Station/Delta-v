using Content.Server._DV.Objectives.Systems;

namespace Content.Server._DV.Objectives.Components;

/// <summary>
///     Kidnap some number of heads. Use the NumberObjective to set the exact number
/// </summary>
[RegisterComponent, Access(typeof(KidnapHeadsConditionSystem))]
public sealed partial class KidnapHeadsConditionComponent: Component;
