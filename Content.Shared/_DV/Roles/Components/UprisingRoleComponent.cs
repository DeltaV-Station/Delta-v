using Content.Shared.Roles.Components;

namespace Content.Shared._DV.Roles;

[RegisterComponent]
public sealed partial class UprisingRoleComponent : BaseMindRoleComponent
{
    [DataField(required: true)]
    public UprisingSide Side;
}

public enum UprisingSide : byte
{
    Insurgent,
    Loyalist,
}

