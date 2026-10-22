using Content.Client.Gameplay;
using Content.Client.Ghost;
using Content.Client.UserInterface.Systems.Gameplay;
using Content.Client.UserInterface.Systems.Ghost.Widgets;
using Content.Shared._DV.Ghost.Roles; // DeltaV - freeform ghosties
using Content.Shared.Ghost;
using Robust.Shared.Console; // Frontier
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Prototypes; // DeltaV - freeform ghosties
using Content.Shared._Corvax.Respawn; // Frontier

namespace Content.Client.UserInterface.Systems.Ghost;

// TODO hud refactor BEFORE MERGE fix ghost gui being too far up
public sealed class GhostUIController : UIController, IOnSystemChanged<GhostSystem>
{
    [Dependency] private readonly IEntityNetworkManager _net = default!;
    [Dependency] private readonly IConsoleHost _consoleHost = default!; // Frontier

    [UISystemDependency] private readonly GhostSystem? _system = default;

    // Updated when get death time from the server.
    private TimeSpan? DeathTime;

    private GhostGui? Gui => UIManager.GetActiveUIWidgetOrNull<GhostGui>();

    public override void Initialize()
    {
        base.Initialize();

        var gameplayStateLoad = UIManager.GetUIController<GameplayStateLoadController>();
        gameplayStateLoad.OnScreenLoad += OnScreenLoad;
        gameplayStateLoad.OnScreenUnload += OnScreenUnload;

        // DeltaV
        SubscribeNetworkEvent<RespawnResetEvent>(OnRespawnReseted);
        SubscribeNetworkEvent<DVSpawnableGhostRoleCooldownUpdateEvent>(OnVentCritterCooldownUpdate); // DeltaV - freeform ghosties
    }

    private void OnScreenLoad()
    {
        LoadGui();
    }

    private void OnScreenUnload()
    {
        UnloadGui();
    }

    public void OnSystemLoaded(GhostSystem system)
    {
        system.PlayerRemoved += OnPlayerRemoved;
        system.PlayerUpdated += OnPlayerUpdated;
        system.PlayerAttached += OnPlayerAttached;
        system.PlayerDetached += OnPlayerDetached;
        system.GhostWarpsResponse += OnWarpsResponse;
        system.GhostRoleCountUpdated += OnRoleCountUpdated;
    }

    public void OnSystemUnloaded(GhostSystem system)
    {
        system.PlayerRemoved -= OnPlayerRemoved;
        system.PlayerUpdated -= OnPlayerUpdated;
        system.PlayerAttached -= OnPlayerAttached;
        system.PlayerDetached -= OnPlayerDetached;
        system.GhostWarpsResponse -= OnWarpsResponse;
        system.GhostRoleCountUpdated -= OnRoleCountUpdated;
    }

    // Begin DeltaV
    private void OnRespawnReseted(RespawnResetEvent ev, EntitySessionEventArgs args)
    {
        DeathTime = ev.Time;
        UpdateGui();
        UpdateRespawn();
    }
    // End DeltaV

    public void UpdateGui()
    {
        if (Gui == null)
        {
            return;
        }

        Gui.Visible = _system?.IsGhost ?? false;
        Gui.Update(_system?.AvailableGhostRoleCount, _system?.Player?.CanReturnToBody);
    }

    // Begin Frontier
    private void UpdateRespawn()
    {
        Gui?.UpdateRespawn(DeathTime);
    }
    // End Frontier

    private void OnPlayerRemoved(GhostComponent component)
    {
        Gui?.Hide();
    }

    private void OnPlayerUpdated(GhostComponent component)
    {
        UpdateGui();
    }

    private void OnPlayerAttached(GhostComponent component)
    {
        if (Gui == null)
            return;

        Gui.Visible = true;
        UpdateRespawn(); // Frontier, DeltaV
        UpdateGui();
    }

    private void OnPlayerDetached()
    {
        Gui?.Hide();
    }

    private void OnWarpsResponse(GhostWarpsResponseEvent msg)
    {
        if (Gui?.TargetWindow is not { } window)
            return;

        window.UpdateWarps(msg.Warps);
        window.Populate();
    }

    private void OnRoleCountUpdated(GhostUpdateGhostRoleCountEvent msg)
    {
        UpdateGui();
    }

    private void OnWarpClicked(NetEntity player)
    {
        var msg = new GhostWarpToTargetRequestEvent(player);
        _net.SendSystemNetworkMessage(msg);
    }

    private void OnGhostnadoClicked()
    {
        var msg = new GhostnadoRequestEvent();
        _net.SendSystemNetworkMessage(msg);
    }

    public void LoadGui()
    {
        if (Gui == null)
            return;

        Gui.RequestWarpsPressed += RequestWarps;
        Gui.ReturnToBodyPressed += ReturnToBody;
        Gui.GhostRolesPressed += GhostRolesPressed;
        Gui.VentCritterPressed += OnVentCritterPressed; // DeltaV - freeform ghosties
        Gui.VentCritterSelected += OnVentCritterSelected; // DeltaV - freeform ghosties
        Gui.TargetWindow.WarpClicked += OnWarpClicked;
        Gui.TargetWindow.OnGhostnadoClicked += OnGhostnadoClicked;
        Gui.GhostRespawnPressed += GuiOnGhostRespawnPressed; // Frontier

        UpdateGui();
    }

    // Begin Frontier
    private void GuiOnGhostRespawnPressed()
    {
        _consoleHost.ExecuteCommand("ghostrespawn");
    }
    // End Frontier

    public void UnloadGui()
    {
        if (Gui == null)
            return;

        Gui.RequestWarpsPressed -= RequestWarps;
        Gui.ReturnToBodyPressed -= ReturnToBody;
        Gui.GhostRolesPressed -= GhostRolesPressed;
        Gui.VentCritterPressed -= OnVentCritterPressed; // DeltaV - freeform ghosties
        Gui.VentCritterSelected -= OnVentCritterSelected; // DeltaV - freeform ghosties
        Gui.TargetWindow.WarpClicked -= OnWarpClicked;
        Gui.GhostRespawnPressed -= GuiOnGhostRespawnPressed; // Frontier

        Gui.Hide();
    }

    private void ReturnToBody()
    {
        _system?.ReturnToBody();
    }

    private void RequestWarps()
    {
        _system?.RequestWarps();
        Gui?.TargetWindow.Populate();
        Gui?.TargetWindow.OpenCentered();
    }

    private void GhostRolesPressed()
    {
        _system?.OpenGhostRoles();
    }

    // Begin DeltaV - freeform ghosties
    private void OnVentCritterPressed()
    {
        _net.SendSystemNetworkMessage(new DVSpawnableGhostRoleCooldownRequestEvent());
    }

    private void OnVentCritterSelected(ProtoId<DVSpawnableGhostRolePrototype> role)
    {
        _net.SendSystemNetworkMessage(new DVSpawnableGhostRoleRequestEvent(role));
    }

    private void OnVentCritterCooldownUpdate(DVSpawnableGhostRoleCooldownUpdateEvent msg, EntitySessionEventArgs args)
    {
        Gui?.VentCritterWindow.SetCooldownEnd(msg.CooldownEnd);
    }
    // End DeltaV - freeform ghosties
}
