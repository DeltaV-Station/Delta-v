using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
using Content.Shared.Contraband;
using System.Text;

namespace Content.Shared._Impstation.Clothing;

/// <summary>
/// Adds examine text to the entity that wears item, for making things obvious.
/// </summary>
public sealed class WearerGetsExamineTextSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WearerGetsExamineTextComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<WearerGetsExamineTextComponent, InventoryRelayedEvent<ExaminedEvent>>(OnExamineWorn);
    }


    private string ConstructExamineText(Entity<WearerGetsExamineTextComponent> entity, bool prefixFallback, EntityUid affecting)
    {
        if (!Exists(affecting))
            return "";

        //parameters (these are the same between both constructions)
        var user = Identity.Entity(affecting, EntityManager);
        var nomen = Identity.Name(affecting, EntityManager);
        var thing = Loc.GetString(entity.Comp.Category);
        var type = Loc.GetString(entity.Comp.Specifier);
        var stringSpec = entity.Comp.Specifier.ToString();
        var shortType = stringSpec.Substring(stringSpec.LastIndexOf('-'));  // necessary for working with colored text...

        var prefix = Loc.GetString(prefixFallback ? "obvious-prefix-default" : entity.Comp.PrefixExamineOnWearer, // uses a different prefix if worn / displayed
                ("user", user),
                ("name", nomen),
                ("thing", thing),
                ("type", type));
        var suffix = Loc.GetString(entity.Comp.ExamineOnWearer,
                ("user", user),
                ("name", nomen),
                ("thing", thing),
                ("type", type),
                ("short-type", shortType));
        return prefix + " " + suffix;
    }

    private void OnExamine(Entity<WearerGetsExamineTextComponent> entity, ref ExaminedEvent args)
    {
        var outString = new StringBuilder(Loc.GetString("obvious-on-item",
            ("used", Loc.GetString("obvious-reveal-default")),
            ("thing", entity.Comp.Category),
            ("me", Identity.Entity(entity, EntityManager))));

        if (entity.Comp.WarnExamine)
        {
            if (TryComp<ContrabandComponent>(entity, out var contra)) // if the item's contra and we're not wearing it yet
            {
                var contraLocId = "obvious-on-item-contra-" + contra.Severity; // apply additional text if the item is contraband to note that displaying it might be really bad
                if (Loc.HasString(contraLocId)) // saves us the trouble of making a switch block for this
                    outString.Append(" " + Loc.GetString(contraLocId));
            }
            var testOut = ConstructExamineText(entity, false, args.Examiner);

            outString.Append("\n" + Loc.GetString("obvious-on-item-for-others",
                ("output", testOut)));
        }

        args.PushMarkup(outString.ToString());
    }

    private void OnExamineWorn(Entity<WearerGetsExamineTextComponent> entity, ref InventoryRelayedEvent<ExaminedEvent> args)
    {
        args.Args.PushMarkup(ConstructExamineText(entity, false, args.Args.Examined));
    }
}
