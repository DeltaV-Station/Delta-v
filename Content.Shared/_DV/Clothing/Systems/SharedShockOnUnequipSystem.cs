using Content.Shared._DV.Clothing.Components;
using Content.Shared.Examine;

namespace Content.Shared._DV.Clothing.Systems;

public abstract class SharedShockOnUnequipSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShockOnUnequipComponent, ExaminedEvent>(OnExamined);
    }
    private void OnExamined(Entity<ShockOnUnequipComponent> selfUnremovableClothing, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("shock-on-unequip-examine"));
    }
}
