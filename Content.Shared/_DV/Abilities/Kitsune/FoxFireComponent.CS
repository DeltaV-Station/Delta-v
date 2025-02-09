namespace Content.Shared.Abilities.Kitsune;

[RegisterComponent, NetworkedComponent]
public sealed partial class FoxFireComponent : Component
{
    public new EntityUid? Owner;
}
