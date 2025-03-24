using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Surgery;

/// <summary>
/// 	Handled by the server when a surgery step is completed in order to deal with sanitization concerns
/// </summary>
[ByRefEvent]
public record struct SurgeryDirtinessEvent(EntityUid User, EntityUid Part, List<EntityUid> Tools, EntityUid Step);
