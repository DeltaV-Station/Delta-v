namespace Content.Shared._Goobstation.Devour.Events;

[ByRefEvent]
public record struct BeforeSelfRevivalEvent(
    EntityUid Target,
    LocId PopupText,
    bool Handled = false,
    bool Cancelled = false);
