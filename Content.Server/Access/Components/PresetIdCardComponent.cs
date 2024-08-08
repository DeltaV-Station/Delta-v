using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server.Access.Components;

[RegisterComponent]
public sealed partial class PresetIdCardComponent : Component
{
    [DataField]
    public ProtoId<JobPrototype>? JobName;

    [DataField]
    public string? IdName;

    /// <summary>
    /// DeltaV: Allow changing the job title, even if it'd be otherwise set by the JobPrototype
    /// </summary>
    [DataField]
    public string? VirtualJobName;

    [ViewVariables(VVAccess.ReadOnly)]
    public string? VirtualJobLocalizedName => (VirtualJobName != null) ? Loc.GetString(VirtualJobName) : null;
    /// <summary>
    /// DeltaV: Allow changing the job icon, even if it'd be otherwise set by the JobPrototype
    /// </summary>
    [DataField]
    public string? VirtualJobIcon;
}
