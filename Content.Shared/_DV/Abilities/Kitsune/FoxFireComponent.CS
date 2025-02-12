using Robust.Shared.GameStates;

namespace Content.Shared._DV.Abilities.Kitsune;

[RegisterComponent, NetworkedComponent]
public sealed partial class FoxFireComponent : Component
{
    public new EntityUid? Owner;
}
