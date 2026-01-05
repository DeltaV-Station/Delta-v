using Content.Shared.Stealth;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Stealth.Components;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedStealthSystem))]
public sealed partial class PreventStealthComponent : Component;
