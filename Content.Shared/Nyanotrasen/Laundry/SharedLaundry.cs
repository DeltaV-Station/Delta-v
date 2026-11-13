using Robust.Shared.Serialization;

namespace Content.Shared.Nyanotrasen.Laundry;

[RegisterComponent]
public sealed partial class SharedWashingMachineComponent : Component { } //Hi, I'm no coder but the word "partial" used to be "sealed" o3o

[Serializable, NetSerializable]
public enum WashingMachineVisualState : byte
{
    Broken,
}
