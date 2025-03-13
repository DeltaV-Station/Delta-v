using Robust.Shared.GameStates;

namespace Content.Shared._DV.Whitelist;

/// <summary>
/// Component added to AI eye entity that lets it get detected by the syndicate ai detector.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AiDetectableComponent : Component;
