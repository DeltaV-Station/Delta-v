using Content.Shared.Actions;

namespace Content.Shared._DV.Vampires.Events;

/// <summary>
/// Raised when a vampire uses their mist-form ability
/// </summary>
public sealed partial class VampireMistFormActionEvent : InstantActionEvent;

/// <summary>
/// Raised when a vampire uses their hypnotic gaze ability
/// </summary>
public sealed partial class VampireHypnoticActionEvent : EntityTargetActionEvent;
