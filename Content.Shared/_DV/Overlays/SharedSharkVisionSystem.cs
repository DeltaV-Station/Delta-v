using Content.Shared._DV.Overlays.Components;
using Content.Shared._Goobstation.Overlays;
using Content.Shared.Actions;

namespace Content.Shared._DV.Overlays;

public sealed class SharedSharkVisionSystem : SwitchableOverlaySystem<SharkVisionComponent, ToggleSharkVisionEvent>;

public sealed partial class ToggleSharkVisionEvent : InstantActionEvent;
