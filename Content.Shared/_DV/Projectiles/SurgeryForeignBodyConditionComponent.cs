using Robust.Shared.GameStates;

namespace Content.Shared._DV.Projectiles;

/// <summary>
/// Requires that a target part has foreign bodies for surgery to be possible
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryForeignBodyConditionComponent : Component;
