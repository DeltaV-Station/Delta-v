using Content.Shared.Roles;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Pager;

/// <summary>
/// Sends event notifications to pages via the device network.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(PageSenderSystem))]
public sealed partial class PageSenderComponent : Component
{
    /// <summary>
    /// Departments to autolink to on a corresponding <see cref="PagerComponent" />
    /// </summary>
    [DataField]
    public HashSet<ProtoId<DepartmentPrototype>> AutoLinkDepartments = new();

    /// <summary>
    /// Jobs to autolink to on a corresponding <see cref="PagerComponent" />
    /// </summary>
    [DataField]
    public HashSet<ProtoId<JobPrototype>> AutoLinkJobs = new();
}
