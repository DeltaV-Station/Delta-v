namespace Content.Shared.Damage.Events; // Put into the main namespace for Damage

/// <summary>
/// Raised when an entity enters stamina crit.
/// </summary>
[ByRefEvent]
public record struct EnterStaminaCritEvent();
