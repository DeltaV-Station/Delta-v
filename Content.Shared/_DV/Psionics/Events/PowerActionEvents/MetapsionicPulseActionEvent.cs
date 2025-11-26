using Content.Shared.Actions;

namespace Content.Shared._DV.Psionics.Events.PowerActionEvents;

/// <summary>
/// This gets fired when someone uses the MetapsionicPulse action.
/// </summary>
[ByRefEvent]
public sealed partial class MetapsionicPulseActionEvent : WorldTargetActionEvent;
