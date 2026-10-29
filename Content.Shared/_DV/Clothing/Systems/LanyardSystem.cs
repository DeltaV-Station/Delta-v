using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

        SubscribeLocalEvent<LanyardComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<LanyardComponent, InventoryRelayedEvent<ExaminedEvent>>(OnWearerExamined);
        SubscribeLocalEvent<LanyardComponent, InventoryRelayedEvent<GetVerbsEvent<ExamineVerb>>>(OnGetExamineVerbs);
    }

    /// <summary>
    /// Called when the item is examined, not the wearer.
    /// </summary>
    private void OnExamined(Entity<LanyardComponent> lanyard, ref ExaminedEvent args)
    {
        using (args.PushGroup(nameof(LanyardComponent)))
        {
            // Get the paper in the lanyard
            TryGetLanyardPaper(lanyard, out var paper);

            // Add basic descriptions (is it empty, is it blank, etc)
            var text = GetLanyardStatusExamineText(paper, lanyard, "item", args.IsInDetailsRange);
            args.PushMarkup(text);

            if (!args.IsInDetailsRange || paper is null || string.IsNullOrWhiteSpace(paper.Content))
                return;

            // push lanyard contents
            args.PushMarkup(Loc.GetString("comp-lanyard-examine-text"));
            var message = CreateLanyardFullExamineText(paper);
            args.PushMessage(message);
        }
    }

    /// <summary>
    /// Called when lanyard wearer is examined.
    /// </summary>
    private void OnWearerExamined(Entity<LanyardComponent> lanyard, ref InventoryRelayedEvent<ExaminedEvent> args)
    {
        using (args.Args.PushGroup(nameof(LanyardComponent)))
        {
            // Get the paper in the lanyard
            TryGetLanyardPaper(lanyard, out var paper);

            // Add basic descriptions (is it empty, is it blank, etc)
            // Also informs the reader that this person is wearing a lanyard
            var text = GetLanyardStatusExamineText(paper, args.Args.Examined, "wearing", args.Args.IsInDetailsRange);
            args.Args.PushMarkup(text);
        }
    }

    private void OnGetExamineVerbs(Entity<LanyardComponent> lanyard, ref InventoryRelayedEvent<GetVerbsEvent<ExamineVerb>> args)
    {
        // Return if there's no paper or if the paper is empty
        if (!TryGetLanyardPaper(lanyard, out var paper)
            || string.IsNullOrWhiteSpace(paper.Content))
            return;

        var isInDetailsRange = _examineSystem.IsInDetailsRange(args.Args.User, args.Args.Target);
        var user = args.Args.User;

        var verb = new ExamineVerb()
        {
            Act = () =>
            {
                var examineText = CreateLanyardFullExamineText(paper);
                _examineSystem.SendExamineTooltip(user, lanyard, examineText, false, false);
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
        ret.AddMarkupPermissive(paper.Content.TrimEnd());

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
    private string GetLanyardStatusExamineText(PaperComponent? paper, EntityUid examined, string examineState, bool inDetailsRange)
    {
        var identity = Identity.Entity(examined, EntityManager);

        // Lanyard is empty
        if (paper is null)
            return Loc.GetString($"comp-lanyard-{examineState}-examine-empty", ("examined", identity));

        // Lanyard is too far away to read
        if (!inDetailsRange)
            return Loc.GetString($"comp-lanyard-{examineState}-examine-too-far", ("examined", identity));

        // Lanyard paper is blank
        if (string.IsNullOrWhiteSpace(paper.Content))
            return Loc.GetString($"comp-lanyard-{examineState}-examine-blank", ("examined", identity));

        // Lanyard has text content
        return Loc.GetString($"comp-lanyard-{examineState}-examine-written", ("examined", identity));
    }

    /// <summary>
    /// Tries to get the paper inside the lanyard.
    /// </summary>
    private bool TryGetLanyardPaper(Entity<LanyardComponent> lanyard, [NotNullWhen(true)] out PaperComponent? paperComponent)
    {
        if (_itemSlots.GetItemOrNull(lanyard, lanyard.Comp.SlotName) is not { Valid: true } item
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
}
