using Robust.Shared.GameStates;

namespace Content.Shared._DV.Psionics.Components;

/// <summary>
/// Entities with this component are shielded from psionic powers.
/// </summary>
/// <remarks>This should solely be used for StatusEffects. For insulative gear, see <seealso cref="PsionicallyInsulativeComponent"/></remarks>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShieldedFromPsionicsComponent : Component;
