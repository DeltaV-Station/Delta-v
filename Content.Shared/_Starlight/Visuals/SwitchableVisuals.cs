using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Shared._Starlight.ItemSwitch.Visuals;

public sealed partial class SwitchableActionEvent : InstantActionEvent;

/// <summary>
///     Generic enum keys for toggle-visualizer appearance data & sprite layers.
/// </summary>
[Serializable, NetSerializable]
public enum ItemSwitchVisuals : byte
{
    Switched,
    Layer
}