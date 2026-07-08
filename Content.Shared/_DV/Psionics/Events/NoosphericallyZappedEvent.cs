namespace Content.Shared._DV.Psionics.Events;

[ByRefEvent]
public record struct NoosphericallyZappedEvent(float RechargeAmount, EntityUid Aggressor, bool CanZap = false);
