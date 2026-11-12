using Robust.Shared.GameStates;

namespace Content.Shared._DV.Psionics.Components;

/// <summary>
/// An implant with this component requires the target to still have <seealso cref="PotentialPsionicComponent.Rolled"/> set to false.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DVImplantRequiresPsionicRollComponent : Component;
