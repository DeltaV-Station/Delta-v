namespace Content.Shared._DV.Weapons.Ranged;

[ByRefEvent]
public record struct ImpulseAppliedEvent(EntityUid User)
{
    public EntityUid Pushed;
}
