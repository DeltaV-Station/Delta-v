using Robust.Shared.Prototypes;

namespace Content.Server._DV.Objectives.Components;

/// <summary>
/// When this objective is completed, duplicate it with a new target.
/// </summary>
[RegisterComponent]
public sealed partial class RerollAfterCompletionComponent : Component
{
    /// <summary>
    /// If true, the objective has already been rerolled.
    /// </summary>
    /// <remarks>
    /// Ideally this shouldn't matter, as we delete the component once its rolled
    /// </remarks>
    public bool Rerolled = false;

    /// <summary>
    /// Tracks a reference of the owner of this objective.
    /// From what I can see, there is no normaly way to get a mind from an objective, as they're usually passed together.
    /// </summary>
    public EntityUid MindUid = default!;

    /// <summary>
    /// Prototype of the objective to use for rerolling.
    /// Probably the same as this entity (If you want a potentially infintie number of objectives), but could be different if you want it to be a different objective.
    /// </summary>
    [DataField]
    public ProtoId<EntityPrototype>? RerollObjectivePrototype;

    /// <summary>
    /// Message to display when the objective is rerolled.
    /// /// If null, no message will be displayed.
    /// </summary>
    [DataField]
    public string? RerollObjectiveMessage = null;
}
