using Robust.Shared.Serialization;

namespace Content.Shared._Starlight.ItemSwitch.Visuals;

// Appearance Data key
[Serializable, NetSerializable]
public enum ItemSwitchLightVisuals : byte
{
    Enabled,
    Color
}
