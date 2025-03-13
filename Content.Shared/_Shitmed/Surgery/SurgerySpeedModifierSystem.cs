using Content.Shared.Armor;
using Content.Shared.Inventory;

namespace Content.Shared._Shitmed.Medical.Surgery;

public sealed partial class SurgerySpeedModifierSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SurgerySpeedModifierComponent, InventoryRelayedEvent<SurgerySpeedModifyEvent>>(OnSpeedModify);
        SubscribeLocalEvent<SurgerySpeedModifierComponent, ArmorExamineEvent>(OnExamineEquipment);
    }

    private void OnSpeedModify(Entity<SurgerySpeedModifierComponent> ent, ref InventoryRelayedEvent<SurgerySpeedModifyEvent> args)
    {
        args.Args.Multiplier *= ent.Comp.SpeedModifier;
    }

    private void OnExamineEquipment(Entity<SurgerySpeedModifierComponent> ent, ref ArmorExamineEvent args)
    {
        args.Msg.PushNewline();
        args.Msg.AddMarkupOrThrow(Loc.GetString("armor-surgery-speed-coefficient-value", ("value", MathF.Round(ent.Comp.SpeedModifier * 100f, 1))));
    }
}
