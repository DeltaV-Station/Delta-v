using Content.Shared.Roles;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Pager;

/// <summary>
/// Receives pages via the device network from page senders.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
[Access(typeof(PagerSystem))]
public sealed partial class PagerComponent : Component
{
    /// <summary>
    /// Devices and their names that this component will display events from.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<string, string> Devices = new();

    /// <summary>
    /// Departments to autolink to on a corresponding <see cref="PageSenderComponent" />
    /// </summary>
    [DataField]
    public HashSet<ProtoId<DepartmentPrototype>> AutoLinkDepartments = new();

    /// <summary>
    /// Jobs to autolink to on a corresponding <see cref="PageSenderComponent" />
    /// </summary>
    [DataField]
    public HashSet<ProtoId<JobPrototype>> AutoLinkJobs = new();
}

[ByRefEvent]
public record struct PageSenderNameEvent(string Name);

[Serializable, NetSerializable]
public sealed class PagerRemoveAddressMessage : BoundUserInterfaceMessage
{
    public readonly string Address;

    public PagerRemoveAddressMessage(string address)
    {
        Address = address;
    }
}
