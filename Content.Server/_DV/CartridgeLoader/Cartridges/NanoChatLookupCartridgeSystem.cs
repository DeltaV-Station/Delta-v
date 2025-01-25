using Content.Server.CartridgeLoader;
using Content.Server.Station.Systems;
using Content.Shared.Access.Components;
using Content.Shared.CartridgeLoader;
using Content.Shared._DV.CartridgeLoader.Cartridges;
using Content.Shared._DV.NanoChat;
using Content.Shared.Radio.Components;
using Content.Server.Power.Components;
using System.Diagnostics.CodeAnalysis;

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

    private bool ServerInRange(Entity<NanoChatLookupCartridgeComponent> cartridge, [NotNullWhen(true)] out EntityUid? station)
    {
        station = _station.GetOwningStation(cartridge);

        if (station is null)
            return false;

        var query =
            EntityQueryEnumerator<TelecomServerComponent, EncryptionKeyHolderComponent, ApcPowerReceiverComponent>();

        while (query.MoveNext(out var uid, out _, out var encryptionKeyHolder, out var power))
        {
            if (power.Powered && _station.GetOwningStation(uid) == station && encryptionKeyHolder.Channels.Contains(cartridge.Comp.RadioChannel))
                return true;
        }

        return false;
    }

    private void UpdateUI(Entity<NanoChatLookupCartridgeComponent> ent, EntityUid loader)
    {
        List<NanoChatRecipient>? contacts;

        if (ServerInRange(ent, out var station))
        {
            contacts = [];

            var query = AllEntityQuery<NanoChatCardComponent, IdCardComponent>();
            while (query.MoveNext(out var entityId, out var nanoChatCard, out var idCardComponent))
            {
                if (nanoChatCard.Number is uint nanoChatNumber && idCardComponent.FullName is string fullName && _station.GetOwningStation(entityId) == station)
                {
                    contacts.Add(new NanoChatRecipient(nanoChatNumber, fullName));
                }
            }
        }
        else
        {
            contacts = null;
        }
        NanoChatLookupUiState state = new NanoChatLookupUiState(contacts);

        _cartridge.UpdateCartridgeUiState(loader, state);
    }
}
