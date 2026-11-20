using Robust.Shared.GameStates;

namespace Content.Shared._DV.Fabricator;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedFabricatorSystem))]
public sealed partial class FabricatorIngredientComponent : Component
{
    [ViewVariables]
    public Entity<FabricatorComponent>? Fab;
}
