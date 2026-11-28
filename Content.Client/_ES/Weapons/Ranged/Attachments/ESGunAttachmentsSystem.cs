using Content.Shared._ES.Weapons.Ranged.Attachments;
using Content.Shared._ES.Weapons.Ranged.Attachments.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Client._ES.Weapons.Ranged.Attachments;

public sealed class ESGunAttachmentsSystem : ESSharedGunAttachmentsSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;

    protected override void OnEntInsertedIntoContainer(Entity<ESAttachableGunComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        base.OnEntInsertedIntoContainer(ent, ref args);

        if (!_timing.IsFirstTimePredicted)
            return;

        if (_userInterface.TryGetOpenUi(ent.Owner, ESAttachableGunUiKey.Key, out var bui))
            bui.Update();
    }

    protected override void OnEntRemovedFromContainer(Entity<ESAttachableGunComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        base.OnEntRemovedFromContainer(ent, ref args);

        if (!_timing.IsFirstTimePredicted)
            return;

        if (_userInterface.TryGetOpenUi(ent.Owner, ESAttachableGunUiKey.Key, out var bui))
            bui.Update();
    }
}
