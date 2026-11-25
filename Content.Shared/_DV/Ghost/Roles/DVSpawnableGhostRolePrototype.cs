using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Ghost.Roles;

[Prototype("dvSpawnableGhostRole")]
public sealed partial class DVSpawnableGhostRolePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    public LocId Name => $"dv-spawnable-{ID}.name";

    public LocId Description => $"dv-spawnable-{ID}.description";

    [DataField(required: true)]
    public LocId Rules;

    [DataField]
    public List<EntProtoId> MindRoles = new() { "MindRoleGhostRoleNeutral" };

    [DataField(required: true)]
    public EntProtoId Entity;
}
