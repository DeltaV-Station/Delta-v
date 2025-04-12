using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;

namespace Content.Server.Wires;

public sealed partial class WiresSystem
{
    public bool HasPower(EntityUid uid)
    {
        // for APCs check if the power wire isn't snipped/pulsed
        if (TryComp<PowerNetworkBatteryComponent>(uid, out var comp))
            return comp.PowerEnabled;

        // for other devices check for APCPowerReceiver
        return this.IsPowered(uid, EntityManager);
    }
}
