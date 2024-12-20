using Content.Server.Objectives.Systems;

namespace Content.Server.DeltaV.Objectives.Components;

/// <summary>
/// Sets the target for <see cref="TargetObjectiveComponent"/> to a random traitor.
/// </summary>
[RegisterComponent, Access(typeof(KillPersonConditionSystem))]
public sealed partial class PickRandomTraitorComponent : Component
{
}
