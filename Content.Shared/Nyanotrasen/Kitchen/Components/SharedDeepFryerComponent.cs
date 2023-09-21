using Robust.Shared.Serialization;

namespace Content.Shared.Kitchen.Components
{
    public abstract class SharedDeepFryerComponent : Component { }

    [Serializable, NetSerializable]
    public enum DeepFryerVisuals : byte
    {
        Bubbling,
    }
}
