using System.Diagnostics.CodeAnalysis;
using Content.Shared.Inventory;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Hands.EntitySystems; // DeltaV - ATMOS Extinguisher Nozzle

namespace Content.Shared.Weapons.Ranged.Systems
{
    public partial class SharedGunSystem
    {
        private bool TryGetHandsSlotEntity(EntityUid uid, ClothingSlotAmmoProviderComponent component, EntityUid user, [NotNullWhen(true)] out EntityUid? slotEntity)
        {
            slotEntity = null;

            if (!component.CheckHands)
                return false;

            foreach (var item in _handsSystem.EnumerateHeld(user))
            {
                if (item == uid)
                    continue;

                if (!_whitelistSystem.IsWhitelistFailOrNull(component.ProviderWhitelist, item))
                {
                    slotEntity = item;
                    return true;
                }
            }

            return false;
        }
    }
}