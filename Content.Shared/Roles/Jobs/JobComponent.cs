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

    // DeltaV #1418 - Inherit job prototype information from a loadout-specified ID
    [DataField]
    public JobComponent? VirtualJob;
}
