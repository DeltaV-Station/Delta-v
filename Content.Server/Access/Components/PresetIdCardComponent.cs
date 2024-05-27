using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server.Access.Components;

[RegisterComponent]
public sealed partial class PresetIdCardComponent : Component
{
    [DataField("job")]
    public ProtoId<JobPrototype>? JobName;

    [DataField("name")]
    public string? IdName;

    /// <summary>
    /// DeltaV: Allow changing the job title, even if it'd be otherwise set by the JobPrototype
    /// </summary>
    [DataField("customJob")]
    public string? CustomJobName;
}
