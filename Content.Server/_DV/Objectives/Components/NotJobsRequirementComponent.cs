using Content.Server.Objectives.Systems;
using Content.Shared.Roles;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

/// <summary>
/// Requires that the player not have a certain job to have this objective.
/// </summary>
[RegisterComponent, Access(typeof(NotJobsRequirementSystem))]
public sealed partial class NotJobsRequirementComponent : Component
{
    /// <summary>
    /// ID of the job to ban from having this objective.
    /// </summary>
    [DataField(required: true, customTypeSerializer: typeof(PrototypeIdListSerializer<JobPrototype>))]
    public List<string> Jobs = new();
}
