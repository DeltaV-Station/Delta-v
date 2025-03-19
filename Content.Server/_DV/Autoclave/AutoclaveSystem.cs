using Content.Server.Power.EntitySystems;
using Content.Shared._DV.Autoclave;
using Content.Shared._DV.Surgery;
using Content.Shared.Power;
using Content.Shared.Storage.Components;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.GameObjects;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Server._DV.Autoclave;

public sealed class AutoclaveSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedEntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly SurgeryCleanSystem _surgeryClean = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AutoclaveComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<AutoclaveComponent, StorageAfterOpenEvent>(OnOpened);
        SubscribeLocalEvent<AutoclaveComponent, StorageAfterCloseEvent>(OnClosed);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<AutoclaveComponent, SurgeryCleansDirtComponent>();

        while (query.MoveNext(out var uid, out var comp, out var cleansDirt))
        {
            if (_timing.CurTime < comp.NextUpdate)
                continue;

            comp.NextUpdate += comp.UpdateInterval;

            var isPowered = _power.IsPowered(uid);
            var isClosed = !_entityStorage.IsOpen(uid);

            if (!(isPowered && isClosed))
                continue;

            SharedEntityStorageComponent? storageComponent = null;
            if (!_entityStorage.ResolveStorage(uid, ref storageComponent))
                continue;

            foreach (var containedEntity in storageComponent.Contents.ContainedEntities)
            {
                _surgeryClean.DoClean((uid, cleansDirt), containedEntity);
            }

            UpdateVisuals(uid, true, true);
        }
    }

    private void UpdateVisuals(EntityUid ent, bool isPowered, bool isClosed)
    {
        SharedEntityStorageComponent? storageComponent = null;
        bool hasDirtyContents =
            _entityStorage.ResolveStorage(ent, ref storageComponent)
                && storageComponent.Contents.ContainedEntities.Any(contained => _surgeryClean.RequiresCleaning(contained));

        var (greenLight, redLight) = (isPowered, isClosed, hasDirtyContents) switch
        {
            (false, _, _) => (false, false),
            (true, false, _) => (false, true),
            (true, true, true) => (true, false),
            (true, true, false) => (false, true),
        };
        _appearance.SetData(ent, AutoclaveVisuals.IsProcessing, greenLight);
        _appearance.SetData(ent, AutoclaveVisuals.IsIdle, redLight);
    }

    private (bool isPowered, bool isClosed) GetVisualData(EntityUid ent)
    {
        var isPowered = _power.IsPowered(ent);
        var isClosed = !_entityStorage.IsOpen(ent);

        return (isPowered, isClosed);
    }

    private void OnPowerChanged(Entity<AutoclaveComponent> ent, ref PowerChangedEvent args)
    {
        var (powered, closed) = GetVisualData(ent);
        UpdateVisuals(ent, powered, closed);
    }

    private void OnOpened(Entity<AutoclaveComponent> ent, ref StorageAfterOpenEvent args)
    {
        var (powered, closed) = GetVisualData(ent);
        UpdateVisuals(ent, powered, closed);
    }

    private void OnClosed(Entity<AutoclaveComponent> ent, ref StorageAfterCloseEvent args)
    {
        var (powered, closed) = GetVisualData(ent);
        UpdateVisuals(ent, powered, closed);
    }
}
