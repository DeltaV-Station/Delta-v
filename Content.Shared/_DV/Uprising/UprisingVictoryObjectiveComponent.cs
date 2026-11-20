using Content.Shared._DV.Roles;

namespace Content.Shared._DV.Uprising;

[RegisterComponent]
public sealed partial class UprisingVictoryObjectiveComponent : Component
{
    [DataField(required: true)]
    public UprisingSide Side;
}
