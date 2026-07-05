using Content.Client.NetworkConfigurator.Systems;
using Content.Shared.DeviceNetwork.Components;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Client._DV.NetworkConfigurator;

/// <summary>
/// Mapping version of <see cref="NetworkConfiguratorLinkOverlay"> that shows the linkages
/// as we move around the map, rather than using component markers.
/// Using markers means it only applies to objects the user is currently in PVS range, but this
/// version continually searches for device networks to draw.
/// </summary>
public sealed class MappingNetworkConfiguratorLinkOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    private readonly DeviceListSystem _deviceListSystem;
    private readonly SharedTransformSystem _transformSystem;

    public Dictionary<EntityUid, Color> Colors = [];

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    public MappingNetworkConfiguratorLinkOverlay()
    {
        IoCManager.InjectDependencies(this);

        _deviceListSystem = _entityManager.System<DeviceListSystem>();
        _transformSystem = _entityManager.System<SharedTransformSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var query = _entityManager.AllEntityQueryEnumerator<DeviceNetworkComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            if (_entityManager.Deleted(uid) || !_entityManager.TryGetComponent(uid, out DeviceListComponent? deviceList))
                continue; // Deleted or doesn't have any device linkages

            var sourceTransform = _entityManager.GetComponent<TransformComponent>(uid);
            if (sourceTransform.MapID == MapId.Nullspace)
            {
                // Can happen if the item is outside the client's view. In that case,
                // we don't have a sensible transform to draw, so we need to skip it.
                continue;
            }

            if (!Colors.TryGetValue(uid, out var color))
            {
                color = new Color(
                    _random.NextByte(0, 255),
                    _random.NextByte(0, 255),
                    _random.NextByte(0, 255));
                Colors.Add(uid, color);
            }

            foreach (var device in _deviceListSystem.GetAllDevices(uid, deviceList))
            {
                if (_entityManager.Deleted(device))
                {
                    continue;
                }

                var linkTransform = _entityManager.GetComponent<TransformComponent>(device);
                if (linkTransform.MapID == MapId.Nullspace)
                {
                    continue;
                }

                args.WorldHandle.DrawLine(_transformSystem.GetWorldPosition(sourceTransform), _transformSystem.GetWorldPosition(linkTransform), color);
            }
        }
    }
}
