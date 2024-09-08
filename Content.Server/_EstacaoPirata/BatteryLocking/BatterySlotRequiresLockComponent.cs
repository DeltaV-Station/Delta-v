namespace Content.Server.SimpleStation14.BatteryLocking;

[RegisterComponent]
public sealed partial class BatterySlotRequiresLockComponent : Component
{
    [DataField]
    public string ItemSlot = string.Empty;
}
