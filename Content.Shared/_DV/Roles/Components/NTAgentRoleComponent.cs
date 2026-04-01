using Content.Shared.Roles;
using Content.Shared.Roles.Components;

namespace Content.Shared._DV.Roles;

/// <summary>
/// Added to mind role entities to tag that they are a Internal Affairs Agent.
/// </summary>
[RegisterComponent]
public sealed partial class NTAgentRoleComponent : BaseMindRoleComponent;
