using System.Linq;
using Content.Shared._DV.Damage.Components;
using Content.Shared.Damage.Systems;

namespace Content.Shared._DV.Damage.Systems;

public sealed class DamageTypeTransferSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<DamageTypeTransferComponent, BeforeDamageChangedEvent>(BeforeDamageChanged);
    }

    private void BeforeDamageChanged(Entity<DamageTypeTransferComponent> ent, ref BeforeDamageChangedEvent args)
    {
        foreach (var (key, value) in args.Damage.DamageDict.ToList())
        {
            // check if we need to swap damage type
            if (!ent.Comp.Transfers.TryGetValue(key, out var toTransfer))
                continue;

            // either add damage type to dictionary or add to existing damage
            if (!args.Damage.DamageDict.TryAdd(toTransfer, value))
            {
                args.Damage.DamageDict[toTransfer] += value;
            }

            args.Damage.DamageDict.Remove(key);
        }
    }
}
