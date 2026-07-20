using Content.Server.Objectives.Systems;

namespace Content.Server.Objectives.Components;

/// <summary>
/// Requires that the player dies to be complete.
/// </summary>
[RegisterComponent, Access(typeof(BecomeGolemConditionSystem))]
public sealed partial class BecomeGolemConditionComponent : Component
{
}