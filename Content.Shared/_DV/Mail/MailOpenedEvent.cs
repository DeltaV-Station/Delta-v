namespace Content.Shared._DV.Mail;

[ByRefEvent]
public record struct MailOpenedEvent(EntityUid? Opener, bool Fraudulent);
