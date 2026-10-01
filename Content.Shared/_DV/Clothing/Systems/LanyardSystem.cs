using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._DV.Clothing.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
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

        SubscribeLocalEvent<LanyardComponent, InventoryRelayedEvent<ExaminedEvent>>(OnExaminedInInventory);
    }

    /// <summary>
    /// Called when the item is examined, not the wearer
    /// </summary>
    private void OnExamined(Entity<LanyardComponent> ent, ref ExaminedEvent args)
    {
        using (args.PushGroup(nameof(LanyardComponent)))
        {
            // Get the paper in the lanyard
            TryGetLanyardPaper(ent.Comp, out var paper);

            // Add basic descriptions (is it empty, is it blank, etc)
            AddLanyardStatusExamineText(ref args, paper);

            if (paper is null || string.IsNullOrWhiteSpace(paper.Content))
                return;

            // push lanyard contents
            args.PushMarkup(Loc.GetString("comp-lanyard-examine-text"));
            args.PushMarkup(paper.Content.TrimEnd());

            // push paper stamps if they exist
            if (GetPaperStampString(paper, out var stampString))
                args.PushMarkup(stampString);
        }
    }

    /// <summary>
    /// Called when lanyard wearer is examined
    /// </summary>
    private void OnExaminedInInventory(Entity<LanyardComponent> ent, ref InventoryRelayedEvent<ExaminedEvent> args)
    {
        using (args.Args.PushGroup(nameof(LanyardComponent)))
        {
            // Get the paper in the lanyard
            TryGetLanyardPaper(ent.Comp, out var paper);

            // Inform the examiner that whoever they're examining is wearing a lanyard
            args.Args.PushMarkup(Loc.GetString("comp-lanyard-wearing-lanyard",
                ("user", Identity.Entity(args.Args.Examined, EntityManager))));

            // Add basic descriptions (is it empty, is it blank, etc)
            AddLanyardStatusExamineText(ref args.Args, paper);

            if (paper is null)
                return;

            // push paper stamps if they exist
            if (GetPaperStampString(paper, out var stampString))
                args.Args.PushMarkup(stampString);
        }
    }

    /// <summary>
    /// Adds lanyard text status to examined event
    /// Tells examiner if lanyard is empty, blank, etc
    /// </summary>
    private void AddLanyardStatusExamineText(ref ExaminedEvent args, PaperComponent? paper)
    {
        if (paper is null)
        {
            // Lanyard is empty
            args.PushMarkup(Loc.GetString("comp-lanyard-examine-empty"));
            return;
        }

        if (!args.IsInDetailsRange)
        {
            // Lanyard is too far away to read
            args.PushMarkup(Loc.GetString("comp-lanyard-examine-too-far"));
            return;
        }

        if (string.IsNullOrWhiteSpace(paper.Content))
        {
            // Lanyard paper is blank
            args.PushMarkup(Loc.GetString("comp-lanyard-examine-blank"));
            return;
        }

        // Lanyard has text content
        args.PushMarkup(Loc.GetString("comp-lanyard-examine-written"));
    }

    /// <summary>
    /// Tries to get the paper inside the lanyard
    /// </summary>
    private bool TryGetLanyardPaper(LanyardComponent lanyardComponent, out PaperComponent? paperComponent)
    {
        if (lanyardComponent.LabelSlot.Item is not { Valid: true } item
            || !TryComp<PaperComponent>(item, out var paper))
        {
            paperComponent = null;
            return false;
        }

        paperComponent = paper;
        return true;
    }

    /// <summary>
    /// Get string that contains all stamps applied to the paper
    /// </summary>
    private bool GetPaperStampString(PaperComponent paper, [NotNullWhen(true)] out string? str)
    {
        if (paper.StampedBy.Count <= 0)
        {
            str = null;
            return false;
        }

        var commaSeparated =
            string.Join(", ", paper.StampedBy.Select(s => Loc.GetString(s.StampedName)));

        str = Loc.GetString(
            "comp-lanyard-examine-detail-stamped-by",
            ("stamps", commaSeparated));
        return true;
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
