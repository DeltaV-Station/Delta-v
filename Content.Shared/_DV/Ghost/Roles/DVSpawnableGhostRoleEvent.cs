using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Ghost.Roles;

[Serializable, NetSerializable]
public sealed class DVSpawnableGhostRoleRequestEvent(ProtoId<DVSpawnableGhostRolePrototype> prototype) : EntityEventArgs
{
    public ProtoId<DVSpawnableGhostRolePrototype> Prototype = prototype;
}

[Serializable, NetSerializable]
public sealed class DVSpawnableGhostRoleCooldownRequestEvent : EntityEventArgs;

[Serializable, NetSerializable]
public sealed class DVSpawnableGhostRoleCooldownUpdateEvent(TimeSpan? cooldownEnd) : EntityEventArgs
{
    public TimeSpan? CooldownEnd = cooldownEnd;
}
