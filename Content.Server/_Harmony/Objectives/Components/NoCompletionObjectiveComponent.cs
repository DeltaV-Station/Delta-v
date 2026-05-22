using Content.Server._Harmony.Objectives.Systems;

namespace Content.Server._Harmony.Objectives.Components;

/// <summary>
/// This objective will always show as 0% complete as it is not intended to be tracked. Used for Conspirator objectives.
/// </summary>
[RegisterComponent, Access(typeof(NoCompletionObjectiveSystem))]
public sealed partial class NoCompletionObjectiveComponent : Component;
