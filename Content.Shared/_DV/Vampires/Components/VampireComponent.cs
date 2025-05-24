using Content.Shared._DV.Vampires.EntitySystems;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Vampires.Components;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedVampireSystem))]
public sealed partial class VampireComponent : Component
{
}
