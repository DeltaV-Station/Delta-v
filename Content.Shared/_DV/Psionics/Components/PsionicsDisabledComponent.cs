using Robust.Shared.GameStates;

namespace Content.Shared._DV.Psionics.Components;

/// <summary>
/// Entities with this component cannot use psionic powers.
/// </summary>
/// <remarks>This should solely be used for StatusEffects. For insulative gear, see <see cref="PsionicallyInsulativeComponent"/></remarks>
[RegisterComponent, NetworkedComponent]
public sealed partial class PsionicsDisabledComponent : Component;
