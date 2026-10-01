using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Metadata;
using Content.Shared._DV.Clothing.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
using Content.Shared.Paper;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Shared._DV.Clothing.Systems;

public sealed class LanyardSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly ExamineSystemShared _examineSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LanyardComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<LanyardComponent, ComponentRemove>(OnComponentRemove);

        SubscribeLocalEvent<LanyardComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<LanyardComponent, InventoryRelayedEvent<ExaminedEvent>>(OnExaminedInInventory);
        SubscribeLocalEvent<LanyardComponent, InventoryRelayedEvent<GetVerbsEvent<ExamineVerb>>>(OnGetExamineVerbs);
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

    private void OnGetExamineVerbs(EntityUid uid, LanyardComponent comp, ref InventoryRelayedEvent<GetVerbsEvent<ExamineVerb>> args)
    {
        // Return if there's no paper or if the paper is empty
        if (!TryGetLanyardPaper(comp, out var paper)
            || string.IsNullOrWhiteSpace(paper.Content))
            return;

        var isInDetailsRange = _examineSystem.IsInDetailsRange(args.Args.User, args.Args.Target);
        var user = args.Args.User;

        var verb = new ExamineVerb()
        {
            Act = () =>
            {
                var examineText = CreateLanyardFullExamineText(paper);
                _examineSystem.SendExamineTooltip(user, uid, examineText, false, false);
            },
            Text = Loc.GetString("comp-lanyard-verb-read"),
            Category = VerbCategory.Examine,
            Disabled = !isInDetailsRange,
            Message = isInDetailsRange ? null : Loc.GetString("comp-lanyard-verb-read-out-of-range"),
            // uses the VV eye as icon
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/vv.svg.192dpi.png")),
        };

        args.Args.Verbs.Add(verb);
    }

    private FormattedMessage CreateLanyardFullExamineText(PaperComponent paper)
    {
        var ret = new FormattedMessage();

        // add paper contents
        var text = paper.Content;
        ret.AddMarkupPermissive(text.TrimEnd());

        // add stamps if we have them
        if (GetPaperStampString(paper, out var stampString))
        {
            ret.PushNewline();
            ret.AddMarkupOrThrow(stampString);
        }

        return ret;
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
    private bool TryGetLanyardPaper(LanyardComponent lanyardComponent, [NotNullWhen(true)] out PaperComponent? paperComponent)
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
        _itemSlots.AddItemSlot(ent, ent.Comp.ContainerName, ent.Comp.LabelSlot);
    }

    private void OnComponentRemove(Entity<LanyardComponent> ent, ref ComponentRemove args)
    {
        _itemSlots.RemoveItemSlot(ent, ent.Comp.LabelSlot);
    }
}
