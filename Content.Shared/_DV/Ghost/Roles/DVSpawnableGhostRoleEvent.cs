using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Ghost.Roles;

[Serializable, NetSerializable]
public sealed class DVSpawnableGhostRoleRequestEvent(ProtoId<DVSpawnableGhostRolePrototype> prototype) : EntityEventArgs
{
    public ProtoId<DVSpawnableGhostRolePrototype> Prototype = prototype;
}
