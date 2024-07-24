using Content.Shared.Chat.V2.Repository;
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

    // DeltaV #1425 - Pass VirtualJobLocalizedName/Icon with the JobComponent to override job information
    [DataField]
    public string? VirtualJobName;
    
    [ViewVariables(VVAccess.ReadOnly)]
    public string? VirtualJobLocalizedName => (VirtualJobName != null) ? Loc.GetString(VirtualJobName) : null;

    [DataField]
    public string? VirtualJobIcon;
    // End of DeltaV code
}
