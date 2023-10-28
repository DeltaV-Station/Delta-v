using Content.Server.Objectives.Systems;

namespace Content.Server.Objectives.Components;

/// <summary>
/// Requires that the player dies to be complete.
/// </summary>
[RegisterComponent, Access(typeof(RaiseGlimmerConditionSystem))]
public sealed partial class RaiseGlimmerConditionComponent : Component
{
    [DataField("target"), ViewVariables(VVAccess.ReadWrite)]
    public int Target;
}