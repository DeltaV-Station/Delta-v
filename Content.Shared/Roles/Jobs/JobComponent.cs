using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Roles.Jobs;

/// <summary>
///     Added to mind entities to hold the data for the player's current job.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class JobComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public ProtoId<JobPrototype>? Prototype;

    // DeltaV #1425 - Pass VirtualJobName/Icon with the JobComponent to override job information
    [DataField]
    public string? VirtualJobName;
    [DataField]
    public string? VirtualJobIcon;
    // End of DeltaV code
}
