using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Forensics.Events;

/// <summary>
/// Simple do after event for allow users to retrieve lodged projectiles
/// from an entity.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class RetrieveProjectilesDoAfterEvent : DoAfterEvent
{
    public override DoAfterEvent Clone()
    {
        return this;
    }
}
