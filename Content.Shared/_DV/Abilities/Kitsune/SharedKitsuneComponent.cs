using Robust.Shared.Serialization;

namespace Content.Shared._DV.Abilities.Kitsune;

[Access(typeof(SharedKitsuneSystem))]
public abstract partial class SharedKitsuneComponent : Component
{

}

[Serializable, NetSerializable]
public enum KitsuneColor
{
    Color,
}
