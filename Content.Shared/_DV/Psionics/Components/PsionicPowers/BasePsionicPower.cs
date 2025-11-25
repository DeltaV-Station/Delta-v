using Robust.Shared.GameStates;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

/// <summary>
/// Entities with this component are psionically insulated from a source.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public abstract partial class BasePsionicPower : Component
{

}
