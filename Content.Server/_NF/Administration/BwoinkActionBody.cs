namespace Content.Server._NF.Administration;

public sealed class BwoinkActionBody
{
    public required string Text { get; init; }
    public required string Username { get; init; }
    public required Guid Guid { get; init; }
    public bool UserOnly { get; init; }
    public required bool WebhookUpdate { get; init; }
    public required string RoleName { get; init; }
    public required string RoleColor { get; init; }
}
