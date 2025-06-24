using Content.Shared.Administration;
using Robust.Shared.Network;

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

public sealed class BwoinkParams
{
    public SharedBwoinkSystem.BwoinkTextMessage Message { get; set; }
    public NetUserId SenderId { get; set; }
    public AdminData? SenderAdmin { get; set; }
    public string SenderName { get; set; }
    public INetChannel? SenderChannel { get; set; }
    public bool UserOnly { get; set; }
    public bool SendWebhook { get; set; }
    public bool FromWebhook { get; set; }
    public string? RoleName { get; set; }
    public string? RoleColor { get; set; }

    public BwoinkParams(
        SharedBwoinkSystem.BwoinkTextMessage message,
        NetUserId senderId,
        AdminData? senderAdmin,
        string senderName,
        INetChannel? senderChannel,
        bool userOnly,
        bool sendWebhook,
        bool fromWebhook,
        string? roleName = null,
        string? roleColor = null)
    {
        Message = message;
        SenderId = senderId;
        SenderAdmin = senderAdmin;
        SenderName = senderName;
        SenderChannel = senderChannel;
        UserOnly = userOnly;
        SendWebhook = sendWebhook;
        FromWebhook = fromWebhook;
        RoleName = roleName;
        RoleColor = roleColor;
    }
}
