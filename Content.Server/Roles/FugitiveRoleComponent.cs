using Content.Shared.Roles;

namespace Content.Server.Roles;

/// <summary>
/// DeltaV - fugitive antag role
/// </summary>
[RegisterComponent, ExclusiveAntagonist]
public sealed partial class FugitiveRoleComponent : AntagonistRoleComponent;
