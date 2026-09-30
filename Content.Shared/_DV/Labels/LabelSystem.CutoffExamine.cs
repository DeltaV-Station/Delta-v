using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.Labels.Components;
using Content.Shared.Paper;
using Content.Shared.Verbs;
using System.Linq;
using Robust.Shared.Utility;
using Content.Shared.IdentityManagement;

namespace Content.Shared.Labels.EntitySystems;

public sealed partial class LabelSystem
{
    [Dependency] private readonly ExamineSystemShared _examineSystem = default!;

    private void AddLanyardFullExamineFromInventory(EntityUid uid, PaperLabelComponent component, InventoryRelayedEvent<GetVerbsEvent<ExamineVerb>> args)
    {
        // check for a paper in the lanyard
        if (component.LabelSlot.Item is not {Valid: true} item
            || !TryComp<PaperComponent>(item, out var paper))
            return;

        // check if another tab is needed
        if (paper.Content.Length <= component.ExamineCharacterLimit)
            return;

        var inDetailsRange = _examineSystem.IsInDetailsRange(args.Args.User, Comp<TransformComponent>(uid).ParentUid);

        var verb = new ExamineVerb()
        {
            Act = () =>
            {
                var examineText = CreateLanyardFullExamineText(uid, paper);
                _examineSystem.SendExamineTooltip(args.Args.User, uid, examineText, false, false);
            },
            Text = Loc.GetString("comp-lanyard-read-full"),
            Category = VerbCategory.Examine,
            Disabled = !inDetailsRange,
            Message = inDetailsRange ? null : Loc.GetString("comp-paper-label-has-cant-read"),
            // uses the VV eye as an icon
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/vv.svg.192dpi.png")),
        };

        args.Args.Verbs.Add(verb);
    }

    private FormattedMessage CreateLanyardFullExamineText(EntityUid uid, PaperComponent paper)
    {
        var ret = new FormattedMessage();

        // get the person wearing the lanyard
        var user = Comp<TransformComponent>(uid).ParentUid;

        ret.AddMarkupOrThrow(
            Loc.GetString("comp-lanyard-has-lanyard",
            ("user", Identity.Entity(user, EntityManager))));
        ret.PushNewline();

        var text = paper.Content;
        ret.AddMarkupPermissive(text.TrimEnd());

        // check for stamps, if none return now
        if (paper.StampedBy.Count <= 0)
            return ret;

        // add stamps if we have them
        var commaSeparated =
            string.Join(", ", paper.StampedBy.Select(s => Loc.GetString(s.StampedName)));

        ret.PushNewline();
        ret.AddMarkupOrThrow(
            Loc.GetString(
                "comp-lanyard-examine-detail-stamped-by",
                ("stamps", commaSeparated))
        );

        return ret;
    }
}
