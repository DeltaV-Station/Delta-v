using Content.Shared._DV.Fishing.Components;
using Content.Shared._Goobstation.Fishing.Components;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Fishing.Systems;

public sealed class FishingVendorSystem : EntitySystem
{
    [Dependency] private readonly FishingPointsSystem _fishingPoints = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FishingVendorComponent, AfterInteractUsingEvent>(OnAfterInteractUsing);
    }

    private void OnAfterInteractUsing(Entity<FishingVendorComponent> vendor, ref AfterInteractUsingEvent args)
    {
        if (args.Handled)
            return;

        // Check if the used item is a fish
        if (!TryComp<FishComponent>(args.Used, out var fishComp))
        {
            _popup.PopupClient(Loc.GetString("fishing-vendor-invalid-item"), vendor, args.User);
            args.Handled = true;
            return;
        }

        // Find the user's ID card
        var idCard = _fishingPoints.TryFindIdCard(args.User);
        if (idCard == null)
        {
            _popup.PopupClient(Loc.GetString("fishing-vendor-no-id-card"), vendor, args.User);
            args.Handled = true;
            return;
        }

        // Handle the fish insertion
        HandleFishInsertion(vendor, args.User, args.Used, fishComp, idCard.Value);
        args.Handled = true;
    }

    private void HandleFishInsertion(
        Entity<FishingVendorComponent> vendor,
        EntityUid user,
        EntityUid fishEntity,
        FishComponent fishComp,
        Entity<FishingPointsComponent?> idCard)
    {
        // Determine if fish is rare based on difficulty threshold
        var isRare = fishComp.FishDifficulty >= vendor.Comp.RareFishThreshold;
        var pointsAwarded = isRare ? vendor.Comp.RareFishPoints : vendor.Comp.BaseFishPoints;
        var fishType = isRare ? "rare" : "base";

        // Add points to the ID card
        if (!_fishingPoints.AddPoints(idCard, pointsAwarded))
        {
            _popup.PopupClient(Loc.GetString("fishing-vendor-failed-to-add-points"), vendor, user);
            return;
        }

        // Delete the fish
        QueueDel(fishEntity);

        // Show success message
        _popup.PopupClient(
            Loc.GetString("fishing-vendor-fish-exchanged",
                ("points", pointsAwarded),
                ("type", fishType)),
            vendor,
            user);
    }
}
