using Robust.Shared.GameStates;

namespace Content.Shared._DV.Projectiles;

/// <summary>
/// Surgery step that removes a foreign body from the part
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryRemoveForeignBodyStepComponent : Component;
