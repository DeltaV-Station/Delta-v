using Content.Shared._DV.Silicons.Charge;
using Content.Shared._EE.Silicon.Components;

namespace Content.Server._DV.Silicons.Charge;

public sealed class SiliconDrainSystem : SharedSiliconDrainSystem
{
    protected override void UpdateChargeIcon(Entity<SiliconComponent> ent, short chargePercent)
    {
        // No-op on Server
    }
}
