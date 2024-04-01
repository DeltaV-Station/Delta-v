using Content.Shared.Roles;

namespace Content.Server.DeltaV.SpaceFerret.Components;

[RegisterComponent, Access(typeof(SpaceFerretSystem)), ExclusiveAntagonist]
public sealed partial class SpaceFerretRoleComponent : AntagonistRoleComponent;
