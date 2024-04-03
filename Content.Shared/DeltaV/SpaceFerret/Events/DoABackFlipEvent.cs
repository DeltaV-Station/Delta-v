using Robust.Shared.Serialization;

namespace Content.Shared.DeltaV.SpaceFerret.Events;

[Serializable] [NetSerializable]
public sealed class DoABackFlipEvent(NetEntity actioner) : EntityEventArgs
{
    public NetEntity Actioner = actioner;
}
