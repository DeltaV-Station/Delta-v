using Content.Shared._DV.Clothing.Components;
using Content.Shared.Examine;

namespace Content.Shared._DV.Clothing;

public abstract class SharedDamageOnUnequipSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DamageOnUnequipComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<DamageOnUnequipComponent> selfUnremovableClothing, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("damage-on-unequip-examine"));
    }
}
