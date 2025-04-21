using Content.Server.Cloning;
using Content.Shared.Cloning.Events;
using Content.Shared.Radio.Components;
using Robust.Server.Containers;

namespace Content.Server._DV.Radio;

public sealed partial class EncryptionKeySystem : EntitySystem
{
    [Dependency] private readonly CloningSystem _clone = default!;
    [Dependency] private readonly ContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EncryptionKeyHolderComponent, CloningEvent>(OnClone);
    }

    // This is basically just for IPCs
    public void OnClone(Entity<EncryptionKeyHolderComponent> keyHolder, ref CloningEvent args)
    {
        if (!args.Settings.CopyInternalStorage)
            return;

        if (!TryComp<EncryptionKeyHolderComponent>(args.CloneUid, out var clonesComp))
            return;

        var keys = keyHolder.Comp.KeyContainer.ContainedEntities;

        foreach (var key in keys)
        {
            var spawned = _clone.CopyItem(key, Transform(keyHolder).Coordinates);
            if (spawned != null)
                _container.InsertOrDrop(spawned.Value, clonesComp.KeyContainer);
        }
    }
}
