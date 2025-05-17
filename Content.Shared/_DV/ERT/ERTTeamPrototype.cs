using Robust.Shared.Prototypes;

namespace Content.Shared._DV.ERT;

/// <summary>
/// Defines the spawners for an ERT team level
/// </summary>
[Prototype("ertTeam")]
public sealed partial class ERTTeamPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// This team's name, e.g. Amber Team, CBURN, Red Team, Gamma Team, etc.
    /// </summary>
    [DataField]
    public LocId Name;

    /// <summary>
    /// The prototypes used to spawn roles for the team
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<ERTRolePrototype>, EntProtoId> Roles = [];
}

/// <summary>
/// Defines a role that can be spawned by an ERT team
/// </summary>
[Prototype("ertRole")]
public sealed partial class ERTRolePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public LocId Name = default!;
}
