using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Tools;
using Content.Shared.Tools.Components;
using Content.Shared.Tools.Systems;
using Content.Shared.Weapons.Ranged.Upgrades;
using Content.Shared.Weapons.Ranged.Upgrades.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared.Weapons.Ranged.Upgrades;

/// <summary>
/// Lets you extract upgrades from a PKA using a crowbar.
/// Same functionality as TG.
/// </summary>
public sealed partial class GunUpgradeSystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedToolSystem _tool = default!;

    public static readonly ProtoId<ToolQualityPrototype> ExtractQuality = "Prying";

    private void TryExtract(Entity<UpgradeableGunComponent> ent, ref AfterInteractUsingEvent args)
    {
        if (args.Handled || !args.CanReach)
            return;

        var tool = args.Used;
        if (!TryComp<ToolComponent>(tool, out var toolComp) || !_tool.HasQuality(tool, ExtractQuality, toolComp))
            return;

        args.Handled = true;

        var user = args.User;
        var upgrades = GetCurrentUpgrades(ent);
        if (upgrades.Count == 0)
        {
            _popup.PopupClient(Loc.GetString("upgradeable-gun-popup-no-upgrades"), ent, user);
            return;
        }

        _popup.PopupClient(Loc.GetString("upgradeable-gun-popup-remove-upgrades"), ent, user);
        _tool.PlayToolSound(tool, toolComp, user);
        TryComp<HandsComponent>(user, out var hands);
        foreach (var upgrade in upgrades)
        {
            _hands.PickupOrDrop(user, upgrade, handsComp: hands);
        }
    }
}
