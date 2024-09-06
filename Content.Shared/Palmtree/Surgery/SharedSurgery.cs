using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Palmtree.Surgery
{
    public abstract partial class SharedSurgerySystem : EntitySystem
    {
        [Serializable, NetSerializable]
        protected sealed partial class SurgeryDoAfterEvent : SimpleDoAfterEvent {}
    }
}
