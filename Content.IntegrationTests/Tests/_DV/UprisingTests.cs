using Content.IntegrationTests.Fixtures;
using Content.IntegrationTests.Fixtures.Attributes;
using Content.Server.Mind;
using Content.Server.Roles;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._DV;

[TestFixture]
public sealed class UprisingTests : GameTest
{
    [SidedDependency(Side.Server)] private MindSystem _mind = default!;
    [SidedDependency(Side.Server)] private RoleSystem _role = default!;

    private static readonly EntProtoId MindRoleLoyalist = "MindRoleLoyalist";
    private static readonly EntProtoId MindRoleInsurgent = "MindRoleInsurgent";

    private static readonly EntProtoId[] MindRoles = [MindRoleLoyalist, MindRoleInsurgent];

    [Test, TestCaseSource(nameof(MindRoles))]
    [RunOnSide(Side.Server)]
    public void TestObjectiveAdditionRemoval(EntProtoId mindRole)
    {
        var body = SSpawn(null);
        var mind = _mind.CreateMind(null);
        _mind.TransferTo(mind, body);
        STrack(mind);

        _role.MindAddRole(mind, mindRole, mind);

        Assert.That(_role.MindGetAllRoleInfo(mind.AsNullable()), Is.Not.Empty);
        Assert.That(mind.Comp.Objectives, Is.Not.Empty);

        _role.MindRemoveRole(mind, mindRole.Id);

        Assert.That(mind.Comp.Objectives, Is.Empty);
        Assert.That(_role.MindGetAllRoleInfo(mind.AsNullable()), Is.Empty);
    }
}
