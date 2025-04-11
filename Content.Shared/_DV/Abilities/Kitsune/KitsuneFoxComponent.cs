using Robust.Shared.GameStates;

namespace Content.Shared._DV.Abilities.Kitsune;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class KitsuneFoxComponent : Component
{
    [DataField, AutoNetworkedField] public Entity<KitsuneFoxComponent?>? Parent;
}
