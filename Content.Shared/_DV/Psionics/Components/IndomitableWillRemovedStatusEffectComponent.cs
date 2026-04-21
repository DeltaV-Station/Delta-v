using Robust.Shared.GameStates;

namespace Content.Shared._DV.Psionics.Components;

/// <summary>
/// This denotes which statusEffect will be removed upon a successful usage of the indomitable will psionic power.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class IndomitableWillRemovedStatusEffectComponent : Component;
