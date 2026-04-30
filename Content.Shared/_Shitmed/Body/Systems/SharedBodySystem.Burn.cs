using Content.Shared.Body.Part;
using Content.Shared.Gibbing;
using Content.Shared.Inventory;

namespace Content.Shared.Body.Systems;

/// <summary>
/// Contains only the logic relevant for burning parts on an entity.
/// Since this is still requiring gibbing, it's best to quarantine the code away from the shared
/// body system before NuBody comes.
/// </summary>
public partial class SharedBodySystem
{
    [Dependency] private GibbingSystem _gibbing = default!;

    public bool BurnPart(EntityUid partId,
        BodyPartComponent? part = null)
    {
        if (!Resolve(partId, ref part, logMissing: false))
            return false;

        if (part.Body is { } bodyEnt)
        {
            if (IsPartRoot(bodyEnt, partId, part: part))
                return false;

            // Todo: Kill this in favor of husking.
            DropSlotContents((partId, part));
            RemovePartChildren((partId, part), bodyEnt);
            foreach (var organ in GetPartOrgans(partId, part))
                _gibbing.Gib(organ.Id);

            _gibbing.Gib(partId);

            if (HasComp<InventoryComponent>(partId))
                foreach (var item in _inventory.GetHandOrInventoryEntities(partId))
                    SharedTransform.AttachToGridOrMap(item);

            QueueDel(partId);
            return true;
        }

        return false;
    }
}
