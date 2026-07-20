using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Storage;
using System.Linq;

namespace Content.Shared._DV.Holosign;

public sealed class ChargeHolosignSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedChargesSystem _charges = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private readonly HashSet<Entity<IComponent>> _placedSigns = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChargeHolosignProjectorComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ChargeHolosignProjectorComponent, BeforeRangedInteractEvent>(OnBeforeInteract);
    }

    private void OnInit(Entity<ChargeHolosignProjectorComponent> ent, ref ComponentInit args)
    {
        // its required, funny test is still funny
        if (string.IsNullOrEmpty(ent.Comp.SignComponentName))
            return;

        ent.Comp.SignComponent = EntityManager.ComponentFactory.GetRegistration(ent.Comp.SignComponentName).Type;
    }

    private void OnBeforeInteract(Entity<ChargeHolosignProjectorComponent> ent, ref BeforeRangedInteractEvent args)
    {
        if (args.Handled || !args.CanReach ||
            HasComp<StorageComponent>(args.Target) || // if it's a storage component like a bag, we ignore usage so it can be stored
            !TryComp<LimitedChargesComponent>(ent, out var charges))
            return;

        // first check if there's any existing holofans to clear
        var coords = args.ClickLocation.SnapToGrid(EntityManager);
        var mapCoords = _transform.ToMapCoordinates(coords);

        _placedSigns.Clear();

        _lookup.GetEntitiesInRange(ent.Comp.SignComponent, mapCoords, 0.25f, _placedSigns);

        if (!ent.Comp.CanPickup || _placedSigns.Count == 0)
            TryPlaceSign((ent, ent, charges), args);
        else
            TryRemoveSign((ent, ent, charges), _placedSigns.First(), args.User);

        args.Handled = true;
    }

    public bool TryPlaceSign(Entity<ChargeHolosignProjectorComponent, LimitedChargesComponent> ent, BeforeRangedInteractEvent args)
    {
        if (!_charges.TryUseCharge((ent, ent.Comp2)))
        {
            _popup.PopupClient(Loc.GetString("charge-holoprojector-no-charges", ("item", ent)), ent, args.User);
            return false;
        }

        var holoUid = EntityManager.PredictedSpawnAtPosition(ent.Comp1.SignProto, args.ClickLocation.SnapToGrid(EntityManager));
        var xform = Transform(holoUid);
        if (!xform.Anchored)
            _transform.AnchorEntity(holoUid, xform); // anchor to prevent any tempering with (don't know what could even interact with it)

        return true;
    }

    public bool TryRemoveSign(Entity<ChargeHolosignProjectorComponent, LimitedChargesComponent> ent, EntityUid sign, EntityUid user)
    {
        if (!ent.Comp1.CanPickup)
            return false;

        _charges.AddCharges((ent, ent.Comp2), 1);

        var userIdentity = Identity.Name(user, EntityManager);
        _popup.PopupPredicted(
            Loc.GetString("charge-holoprojector-reclaim", ("sign", sign)),
            Loc.GetString("charge-holoprojector-reclaim-others", ("sign", sign), ("user", userIdentity)),
            ent,
            user);

        EntityManager.PredictedDeleteEntity(sign);
        return true;
    }
}
