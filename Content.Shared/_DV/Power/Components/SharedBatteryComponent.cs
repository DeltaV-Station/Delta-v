namespace Content.Shared._DV.Power.Components
{
    public abstract partial class SharedBatteryComponent : Component
    {
        public abstract float CurrentCharge { get; set; }
    }
    [ByRefEvent]
    public readonly record struct ChargeChangedEvent(float Charge, float MaxCharge);
}
