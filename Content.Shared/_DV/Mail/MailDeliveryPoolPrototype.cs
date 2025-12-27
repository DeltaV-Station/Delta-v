using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Mail;

/// <summary>
/// Generic random weighting dataset to use.
/// </summary>
[Prototype]
public sealed class MailDeliveryPoolPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    /// <summary>
    /// Mail that can be sent to everyone.
    /// </summary>
    [DataField]
    public Dictionary<EntProtoId, float> Everyone = new();

    /// <summary>
    /// Mail that can be sent only to specific jobs.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<JobPrototype>, Dictionary<EntProtoId, float>> Jobs = new();

    /// <summary>
    /// Mail that can be sent only to specific departments.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<DepartmentPrototype>, Dictionary<EntProtoId, float>> Departments = new();
}
