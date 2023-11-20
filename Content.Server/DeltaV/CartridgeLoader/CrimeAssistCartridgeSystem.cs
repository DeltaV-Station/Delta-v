using Content.Shared.CartridgeLoader;
using Content.Server.CartridgeLoader;

namespace Content.Server.DeltaV.CartridgeLoader.Cartridges;

public sealed class CrimeAssistCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem? _cartridgeLoaderSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CrimeAssistCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
        SubscribeLocalEvent<CrimeAssistCartridgeComponent, CartridgeMessageEvent>(OnMessage);
    }

    /// <summary>
    /// This gets called when the ui fragment needs to be updated for the first time after activating
    /// </summary>
    private void OnUiReady(EntityUid uid, CartridgeComponent component, CartridgeUiReadyEvent args)
    {
        UpdateUiState(uid, args.Loader, component);
    }
}
