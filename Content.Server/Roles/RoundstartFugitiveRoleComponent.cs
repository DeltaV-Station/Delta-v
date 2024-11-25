using Content.Shared.Roles;

namespace Content.Server.Roles;

/// <summary>
/// Taken from ThiefRoleComponent
///     Added to mind role entities to tag that they are a roundstart fugitive.
/// </summary>
[RegisterComponent]
public sealed partial class RoundstartFugitiveRoleComponent : BaseMindRoleComponent
{
}
