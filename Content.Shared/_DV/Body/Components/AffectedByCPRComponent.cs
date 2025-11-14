using Robust.Shared.GameStates;

namespace Content.Shared._DV.Body.Components;

/// <summary>
/// For patients currently under the effect of CPR.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AffectedByCPRComponent : Component;
