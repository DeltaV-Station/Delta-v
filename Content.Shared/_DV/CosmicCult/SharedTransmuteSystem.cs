using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared._DV.CosmicCult.Prototypes;
using Content.Shared.Examine;
using Content.Shared.Verbs;
using Content.Shared.Localizations;
using Robust.Shared.Prototypes;
using Robust.Shared.Spawners;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._DV.CosmicCult;

public abstract class SharedTransmuteSystem : EntitySystem
{
    [Dependency] private readonly SharedCosmicCultSystem _cosmicCult = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ExamineSystemShared _examine = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CosmicTransmutableComponent, GetVerbsEvent<ExamineVerb>>(OnDetailedExamine);
    }

    private void OnDetailedExamine(EntityUid ent, CosmicTransmutableComponent component, ref GetVerbsEvent<ExamineVerb> args)
    {
        if (!args.CanInteract)
            return;


        var result = _proto.Index(component.TransumtesTo);
        var glyph = _proto.Index(component.RequiredGlyphType);
        //if (!EntityIsCultist(ent)) //non-cultists don't need to know this anyway
        //    return;
        String text = Loc.GetString("cosmic-examine-transmutable", ("result", result), ("glyph", glyph));
        var iconTexture = "/Textures/Interface/VerbIcons/lock-red.svg.192dpi.png";
        var examineMarkup = GetTransmuteExamine(text);
        _examine.AddHoverExamineVerb(args,
            component,
            Loc.GetString("contraband-examinable-verb-text"),
            examineMarkup.ToMarkup(),
            iconTexture);
    }

    private FormattedMessage GetTransmuteExamine(String text)
    {
        var msg = new FormattedMessage();
        msg.AddMarkupOrThrow(text);
        return msg;
    }
}
