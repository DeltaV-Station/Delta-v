using Content.Shared._DV.Clothing.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Examine;
using Content.Shared.Paper;

namespace Content.Shared._DV.Clothing.Systems;

public sealed class LanyardSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;

    public const string ContainerName = "lanyard_label";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LanyardComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<LanyardComponent, ComponentRemove>(OnComponentRemove);

        SubscribeLocalEvent<LanyardComponent, ExaminedEvent>(OnExamined);
    }

    /// <summary>
    /// Called when the item is examined, not the wearer
    /// </summary>
    private void OnExamined(Entity<LanyardComponent> ent, ref ExaminedEvent args)
    {
        using (args.PushGroup(nameof(LanyardComponent)))
        {
            if (ent.Comp.LabelSlot.Item is not { Valid: true } item
                || !TryComp<PaperComponent>(item, out var paper))
            {
                // Lanyard is empty
                args.PushMarkup(Loc.GetString("comp-lanyard-item-examine-empty"));
                return;
            }

            if (!args.IsInDetailsRange)
            {
                // Lanyard is too far away to read
                args.PushMarkup(Loc.GetString("comp-lanyard-item-examine-too-far"));
                return;
            }

            if (string.IsNullOrWhiteSpace(paper.Content))
            {
                // Lanyard paper is blank
                args.PushMarkup(Loc.GetString("comp-lanyard-item-examine-blank"));
                return;
            }

            // push lanyard contents
            args.PushMarkup("comp-lanyard-item-examine");
            args.PushMarkup(paper.Content.TrimEnd());
            // TODO stamps
        }
    }

    private void OnComponentInit(Entity<LanyardComponent> ent, ref ComponentInit args)
    {
        _itemSlots.AddItemSlot(ent, ContainerName, ent.Comp.LabelSlot);
    }

    private void OnComponentRemove(Entity<LanyardComponent> ent, ref ComponentRemove args)
    {
        _itemSlots.RemoveItemSlot(ent, ent.Comp.LabelSlot);
    }
}
