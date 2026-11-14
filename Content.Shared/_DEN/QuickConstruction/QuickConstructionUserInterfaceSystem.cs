using Content.Shared.Whitelist;
using Content.Shared._DEN.QuickConstruction.Components;
using Content.Shared.UserInterface;
using Robust.Shared.Prototypes;
using Content.Shared._DEN.QuickConstruction.Prototypes;
using Content.Shared.Construction.Prototypes;

namespace Content.Shared._DEN.QuickConstruction;

public sealed partial class QuickConstructionUserInterfaceSystem : EntitySystem
{    
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private EntityWhitelistSystem _whitelistSystem = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<QuickConstructableComponent, ActivatableUIOpenAttemptEvent>(OnUIOpenAttempt);
    }

    // Check if menu will actually show. If not, see if we can do any other actions.
    public void OnUIOpenAttempt(Entity<QuickConstructableComponent> ent, ref ActivatableUIOpenAttemptEvent args)
    {
        // could not get component or its category, so nothing will open
        if (!TryComp<QuickConstructableComponent>(ent, out var comp)
            || !_prototypeManager.TryIndex(comp.Category, out var baseCategory))
        {
            args.Cancel();
            return;
        }

        Queue<ProtoId<QuickConstructionCategoryPrototype>> categoryEntries = new Queue<ProtoId<QuickConstructionCategoryPrototype>>(baseCategory.CategoryEntries);
        List<ProtoId<ConstructionPrototype>> constructionEntries = [.. baseCategory.ConstructionEntries];
        // try and find an entry that indicates the menu will show
        while (constructionEntries.Count > 0 || categoryEntries.Count > 0)
        {
            // Check if player can construct any immediate construction entries.
            foreach (var constructionEntry in constructionEntries)
            {
                if (!_prototypeManager.TryIndex(constructionEntry, out var constructionProto))
                    continue;

                if (_whitelistSystem.IsWhitelistPassOrNull(constructionProto.EntityWhitelist, args.User))
                {
                    // we can build something, so the menu will show. we're all good
                    return;
                }
            }

            constructionEntries = [];
            
            // iterate a category entry to find more construction entries to check
            if (categoryEntries.Count > 0)
            {
                var categoryEntry = categoryEntries.Dequeue();
                if (!_prototypeManager.TryIndex(categoryEntry, out var categoryProto))
                    continue;

                foreach (var subcategory in categoryProto.CategoryEntries)
                {
                    categoryEntries.Enqueue(subcategory);
                }

                constructionEntries.AddRange(categoryProto.ConstructionEntries);
            }
        }
        // you can't build anything. so sad ;-;
        args.Cancel();
    }
}
