using Content.Shared.Actions;

namespace Content.Shared._DV.Stunnable.Events;

/// <summary>
/// Simple action for toggling the K9ShockJaws, raised when the user triggers the action.
/// </summary>
[ByRefEvent]
public sealed partial class ToggleK9ShockJawsEvent : InstantActionEvent;
