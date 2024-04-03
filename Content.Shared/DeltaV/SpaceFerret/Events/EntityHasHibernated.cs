using Robust.Shared.Serialization;

namespace Content.Shared.DeltaV.SpaceFerret.Events;

[Serializable] [NetSerializable]
public sealed class EntityHasHibernated(NetEntity hibernator) : EntityEventArgs
{
    public NetEntity Hibernator = hibernator;
}
