using Content.Server.Chemistry.Components;
using Content.Server.Chemistry.EntitySystems;
using Content.Server.Fluids.Components;
using Content.Server.Gravity;
using Content.Server.Popups;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids;
using Content.Shared.Interaction;
using Content.Shared.Timing;
using Content.Shared.Vapor;
using Content.Shared.Chemistry.EntitySystems;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using System.Numerics;
using Robust.Shared.Map;
using Content.Shared.Inventory; //  DeltaV - ATMOS Extinguisher Nozzle
using Content.Shared.Whitelist; //  DeltaV - ATMOS Extinguisher Nozzle
using Content.Shared.Hands.EntitySystems; // DeltaV - ATMOS Extinguisher Nozzle

namespace Content.Server.Fluids.EntitySystems
{
    public sealed partial class SpraySystem : EntitySystem
    {
        private (EntityUid sprayOwner, string solutionName) AtmosNozzleSpray(Entity<SprayComponent> entity, EntityUid user)
        {
            var sprayOwner = entity.Owner;
            var solutionName = SprayComponent.SolutionName;

            if (entity.Comp.ExternalContainer == true)
            {
                bool foundContainer = false;

                foreach (var item in _handsSystem.EnumerateHeld(user))
                {
                    if (item == entity.Owner)
                    {
                        continue;
                    }

                    if (!_whitelistSystem.IsWhitelistFailOrNull(entity.Comp.ProviderWhitelist, item) &&
                        _solutionContainer.TryGetSolution(item, SprayComponent.TankSolutionName, out _, out _))
                    {
                        sprayOwner = item;
                        solutionName = SprayComponent.TankSolutionName;
                        foundContainer = true;
                        break;
                    }
                }

                if (!foundContainer && _inventory.TryGetContainerSlotEnumerator(user, out var enumerator, entity.Comp.TargetSlot))
                {
                    while (enumerator.NextItem(out var item))
                    {
                        if (!_whitelistSystem.IsWhitelistFailOrNull(entity.Comp.ProviderWhitelist, item) &&
                            _solutionContainer.TryGetSolution(item, SprayComponent.TankSolutionName, out _, out _))
                        {
                            sprayOwner = item;
                            solutionName = SprayComponent.TankSolutionName;
                            foundContainer = true;
                            break;
                        }
                    }
                }
            }

            return (sprayOwner, solutionName);
        }
    }
}