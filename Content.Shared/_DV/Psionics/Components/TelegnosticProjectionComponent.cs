using Content.Shared._DV.Psionics.Systems.PsionicPowers;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Psionics.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedTelegnosisPowerSystem))]
public sealed partial class TelegnosticProjectionComponent : Component;
