using Content.Shared.Actions;

namespace Content.Shared._DV.Psionics.Events.PowerActionEvents;

/// <summary>
/// This gets fired when someone uses the PsionicEruption action.
/// </summary>
[ByRefEvent]
public sealed partial class PsionicEruptionPowerActionEvent : InstantActionEvent;
