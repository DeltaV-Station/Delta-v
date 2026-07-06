using Content.Shared.EntityTable.EntitySelectors;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.InnateTools;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(DVInnateToolsSystem))]
public sealed partial class DVInnateToolsComponent : Component
{
    [DataField(required: true)]
    public EntityTableSelector Tools;

    [DataField, AutoNetworkedField]
    public int Provided;
}
