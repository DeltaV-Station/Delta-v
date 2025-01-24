using Content.Server.CartridgeLoader;
using Content.Server.Station.Systems;
using Content.Shared.Access.Components;
using Content.Shared.CartridgeLoader;
using Content.Shared._DV.CartridgeLoader.Cartridges;
using Content.Shared._DV.NanoChat;

namespace Content.Server._DV.CartridgeLoader.Cartridges;

public sealed class NanoChatLookupCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem _cartridge = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NanoChatLookupCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady, [typeof(CartridgeLoaderSystem), typeof(StationSystem)]);
    }

    private void OnUiReady(Entity<NanoChatLookupCartridgeComponent> ent, ref CartridgeUiReadyEvent args)
    {
        UpdateUI(ent, args.Loader);
    }

    private void UpdateUI(Entity<NanoChatLookupCartridgeComponent> ent, EntityUid loader)
    {
        Log.Debug("UpdateUI");
        var contacts = new List<NanoChatRecipient>();

        if (_station.GetOwningStation(loader) is { } station)
        {
            ent.Comp.Station = station;

            var query = AllEntityQuery<NanoChatCardComponent, IdCardComponent>();
            while (query.MoveNext(out var entityId, out var nanoChatCard, out var idCardComponent))
            {
                if (nanoChatCard.Number is uint nanoChatNumber && idCardComponent.FullName is string fullName && _station.GetOwningStation(entityId) == station)
                {
                    contacts.Add(new NanoChatRecipient(nanoChatNumber, fullName));
                }
            }
            Log.Debug($"UpdateUI - {contacts.Count} contacts found");
        }
        else
        {
            Log.Debug("UpdateUI - Not on station");
        }

        var state = new NanoChatLookupUiState(contacts);
        _cartridge.UpdateCartridgeUiState(loader, state);
    }
}
