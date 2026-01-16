using Robust.Shared.GameStates;

namespace Content.Shared._DV.Mail;

/// <summary>
/// Used to mark entities that can receive mail.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MailReceiverComponent : Component;
