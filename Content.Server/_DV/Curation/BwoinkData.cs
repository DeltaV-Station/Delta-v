using Content.Shared._DV.Curation;
using Content.Shared.Administration;
using Robust.Shared.Network;

namespace Content.Server._DV.Curation;

public sealed class CwoinkParams
{
    public CwoinkTextMessage Message { get; set; }
    public NetUserId SenderId { get; set; }
    public AdminData? SenderAdmin { get; set; }
    public string SenderName { get; set; }
    public INetChannel? SenderChannel { get; set; }
    public bool UserOnly { get; set; }
    public bool SendWebhook { get; set; }
    public bool FromWebhook { get; set; }
    public string? RoleName { get; set; }
    public string? RoleColor { get; set; }

    public CwoinkParams(
        CwoinkTextMessage message,
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
