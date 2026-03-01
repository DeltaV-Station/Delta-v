using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.RemoteControl.Events;


[Serializable, NetSerializable]
public sealed partial class RemoteControlBindChangeDoAfterEvent : SimpleDoAfterEvent
{
    public readonly bool Binding;
    public RemoteControlBindChangeDoAfterEvent(bool binding = true)
    {
        Binding = binding;
    }
}
