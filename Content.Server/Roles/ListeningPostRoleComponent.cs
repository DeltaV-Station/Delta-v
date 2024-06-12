using Content.Shared.Roles;

namespace Content.Server.Roles;

/// <summary>
/// DeltaV - listening post ops have their own role
/// </summary>
[RegisterComponent, ExclusiveAntagonist]
public sealed partial class ListeningPostRoleComponent : AntagonistRoleComponent;
