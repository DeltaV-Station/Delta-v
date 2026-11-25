using System.Linq;
using Content.Shared._DV.Forensics;
using Robust.Client.GameObjects;
using Robust.Client.ResourceManagement;
using Robust.Shared.Containers;
using Robust.Shared.Utility;

namespace Content.Client._DV.Forensics;

public sealed class DVItemSlotVisualsSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    private static readonly ResPath TextureRoot = new("/Textures");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DVItemSlotVisualsComponent, EntInsertedIntoContainerMessage>(OnInsertedIntoContainer);
        SubscribeLocalEvent<DVItemSlotVisualsComponent, EntRemovedFromContainerMessage>(OnRemovedFromContainer);
    }

    private void RefreshVisuals(Entity<DVItemSlotVisualsComponent> ent)
    {
        var slot = _container.EnsureContainer<ContainerSlot>(ent, ent.Comp.ItemSlot);
        if (slot.ContainedEntity is { } contained)
        {
            _sprite.CopySprite(contained, ent.Owner);
            _sprite.AddLayer(ent.Owner, ent.Comp.FilledSprite);
        }
        else
        {
            var sprite = Comp<SpriteComponent>(ent);
            var count = sprite.AllLayers.Count();
            for (var i = count - 1; i >= 0; i--)
            {
                _sprite.RemoveLayer((ent, sprite), i);
            }

            if (ent.Comp.UnfilledSprite is not SpriteSpecifier.Rsi rsi)
            {
                throw new InvalidOperationException($"{ToPrettyString(ent)} has an unfilled sprite that's not an RSI");
            }

            if (!_resourceCache.TryGetResource<RSIResource>(TextureRoot / rsi.RsiPath, out var res))
            {
                throw new InvalidOperationException($"{ToPrettyString(ent)} has invalid RSI: {TextureRoot / rsi.RsiPath}");
            }

            _sprite.SetBaseRsi(ent.Owner, res.RSI);
            _sprite.AddLayer(ent.Owner, ent.Comp.UnfilledSprite);
        }
    }

    private void OnInsertedIntoContainer(Entity<DVItemSlotVisualsComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != ent.Comp.ItemSlot)
            return;

        RefreshVisuals(ent);
    }

    private void OnRemovedFromContainer(Entity<DVItemSlotVisualsComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != ent.Comp.ItemSlot)
            return;

        RefreshVisuals(ent);
    }
}
