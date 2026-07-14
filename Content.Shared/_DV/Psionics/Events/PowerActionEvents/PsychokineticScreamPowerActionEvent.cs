using Content.Shared.Actions;

namespace Content.Shared._DV.Psionics.Events.PowerActionEvents;

/// <summary>
/// This gets fired when someone uses the ShatterLightsPower action.
/// </summary>
[ByRefEvent]
public sealed partial class PsychokineticScreamPowerActionEvent : InstantActionEvent;
