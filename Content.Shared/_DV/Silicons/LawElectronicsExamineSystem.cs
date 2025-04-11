using Content.Shared._DV.Whitelist;
using Content.Shared.Examine;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Silicons.Laws;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Silicons;

public sealed class LawElectronicsExamineSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SiliconLawProviderComponent, ExaminedEvent>(OnLawExamined);
    }

    private void OnLawExamined(Entity<SiliconLawProviderComponent> ent, ref ExaminedEvent args)
    {
        if (!HasComp<ElectronicsComponent>(ent))
            return;

        if (ent.Comp.Lawset is {} lawset)
        {
            foreach (var law in lawset.Laws)
            {
                args.PushMarkup(Loc.GetString("law-electronics-examine-law", ("order", law.Order), ("text", Loc.GetString(law.LawString))));
            }
            args.PushMarkup(Loc.GetString("law-electronics-examine-obeys-to", ("owner", Loc.GetString(lawset.ObeysTo))));
        }
        else
        {
            var proto = _prototype.Index(ent.Comp.Laws);
            foreach (var law in proto.Laws)
            {
                var lawProto = _prototype.Index<SiliconLawPrototype>(law);
                args.PushMarkup(Loc.GetString("law-electronics-examine-law", ("order", lawProto.Order), ("text", Loc.GetString(lawProto.LawString))));
            }
            args.PushMarkup(Loc.GetString("law-electronics-examine-obeys-to", ("owner", Loc.GetString(proto.ObeysTo))));
        }
    }
}
