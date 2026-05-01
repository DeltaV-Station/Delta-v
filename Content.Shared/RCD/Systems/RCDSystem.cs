using Content.Shared.Administration.Logs;
using Content.Shared.Charges.Systems;
using Content.Shared.Construction;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.RCD.Components;
using Content.Shared.Tag;
using Content.Shared.Tiles;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using System.Linq;
using Content.Shared.Atmos.EntitySystems; //DeltaV - RPD
using Content.Shared.Atmos.Components; //DeltaV - RPD
using Content.Shared.Hands.Components; //DeltaV - RPD
using System.Numerics; //DeltaV - RPD
using Content.Shared.Verbs; //DeltaV - RPD
using Robust.Shared.Utility; //DeltaV - RPD
using Content.Shared.NodeContainer; //DeltaV - RPD
using Content.Shared.Atmos; //DeltaV - RPD

namespace Content.Shared.RCD.Systems;

public sealed class RCDSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefMan = default!;
    [Dependency] private readonly FloorTileSystem _floors = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedChargesSystem _sharedCharges = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly TileSystem _tile = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly TagSystem _tags = default!;
    [Dependency] private readonly SharedAtmosPipeLayersSystem _pipeLayersSystem = default!; //DeltaV - RPD
    [Dependency] private readonly IEntityManager _entityManager = default!; //DeltaV - RPD
    [Dependency] private readonly PipeRestrictOverlapSystem _pipeOverlap = default!; //DeltaV - RPD

    private readonly int _instantConstructionDelay = 0;
    private readonly EntProtoId _instantConstructionFx = "EffectRCDConstruct0";
    private readonly ProtoId<RCDPrototype> _deconstructTileProto = "DeconstructTile";
    private readonly ProtoId<RCDPrototype> _deconstructLatticeProto = "DeconstructLattice";
    private static readonly ProtoId<TagPrototype> CatwalkTag = "Catwalk";

    private HashSet<EntityUid> _intersectingEntities = new();
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RCDComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<RCDComponent, ComponentStartup>(OnStartup); //DeltaV - RPD
        SubscribeLocalEvent<RCDComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<RCDComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<RCDComponent, RCDDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<RCDComponent, DoAfterAttemptEvent<RCDDoAfterEvent>>(OnDoAfterAttempt);
        SubscribeLocalEvent<RCDComponent, RCDSystemMessage>(OnRCDSystemMessage);
        SubscribeNetworkEvent<RCDConstructionGhostRotationEvent>(OnRCDconstructionGhostRotationEvent);
        SubscribeNetworkEvent<RCDConstructionGhostFlipEvent>(OnRCDConstructionGhostFlipEvent); //DeltaV - RPD
        SubscribeNetworkEvent<RPDEyeRotationEvent>(OnRPDEyeRotationEvent); //DeltaV - RPD
        SubscribeLocalEvent<RCDComponent, GetVerbsEvent<UtilityVerb>>(OnGetUtilityVerb); //DeltaV - RPD
        SubscribeLocalEvent<RCDComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAlternativeVerb); //DeltaV - RPD
    }

    #region Event handling

    private void OnMapInit(EntityUid uid, RCDComponent component, MapInitEvent args)
    {
        // On map startup, set the RCD to its first available recipe
        if (component.AvailablePrototypes.Count > 0)
        {
            //DeltaV - RPD Begin
            if (component.IsRpd)
                component.ProtoId = "PipeStraight";
            else
                component.ProtoId = component.AvailablePrototypes.ElementAt(0);
            //DeltaV - RPD End
            Dirty(uid, component);

            return;
        }

        // The RCD has no valid recipes somehow? Get rid of it
        QueueDel(uid);
    }

    //DeltaV - RPD Begin
    private void OnStartup(EntityUid uid, RCDComponent component, ComponentStartup args)
    {
        UpdateCachedPrototype(uid, component);
        Dirty(uid, component);

        return;
    }
    //DeltaV - RPD End

    private void OnRCDSystemMessage(EntityUid uid, RCDComponent component, RCDSystemMessage args)
    {
        // Exit if the RCD doesn't actually know the supplied prototype
        if (!component.AvailablePrototypes.Contains(args.ProtoId))
            return;

        if (!_protoManager.Resolve<RCDPrototype>(args.ProtoId, out var prototype))
            return;

        // Set the current RCD prototype to the one supplied
        component.ProtoId = args.ProtoId;
        UpdateCachedPrototype(uid, component); //DeltaV - RPD

        _adminLogger.Add(LogType.RCD, LogImpact.Low, $"{args.Actor} set RCD mode to: {prototype.Mode} : {prototype.Prototype}");

        Dirty(uid, component);
    }

    private void OnExamine(EntityUid uid, RCDComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        UpdateCachedPrototype(uid, component); //DeltaV - RPD
        var prototype = component.CachedPrototype; //DeltaV - RPD

        var msg = Loc.GetString("rcd-component-examine-mode-details", ("mode", Loc.GetString(prototype.SetName)));

        if (prototype.Mode == RcdMode.ConstructTile || prototype.Mode == RcdMode.ConstructObject)
        {
            var name = Loc.GetString(prototype.SetName);

            if (prototype.Prototype != null &&
                _protoManager.TryIndex(prototype.Prototype, out var proto)) // don't use Resolve because this can be a tile
                name = proto.Name;

            msg = Loc.GetString("rcd-component-examine-build-details", ("name", name));
        }

        args.PushMarkup(msg);
        //DeltaV - RPD Begin
        if (component.IsRpd)
        {
            var modeLoc = $"rcd-rpd-mode-{component.CurrentMode.ToString().ToLowerInvariant()}";
            args.PushMarkup(Loc.GetString("rcd-component-examine-rpd-mode", ("mode", Loc.GetString(modeLoc))));
        }
    }

    private void OnRPDEyeRotationEvent(RPDEyeRotationEvent ev, EntitySessionEventArgs session)
    {
        var uid = GetEntity(ev.NetEntity);

        if (session.SenderSession.AttachedEntity is not { } player)
            return;

        if (_hands.GetActiveItem(player) != uid)
            return;

        if (!TryComp<RCDComponent>(uid, out var rcd))
            return;

        // Update the layer if different
        if (rcd.LastKnownEyeRotation != ev.EyeRotation)
        {
            rcd.LastKnownEyeRotation = ev.EyeRotation;
        }
    }

    private void OnGetUtilityVerb(EntityUid uid, RCDComponent component, GetVerbsEvent<UtilityVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !component.IsRpd)
            return;

        var verb = new UtilityVerb
        {
            Act = () => SwitchPipeMode(uid, component, args.User),
            Text = Loc.GetString("rcd-verb-switch-mode"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/settings.svg.192dpi.png")),
            Impact = LogImpact.Low
        };

        args.Verbs.Add(verb);
    }

    private void OnGetAlternativeVerb(EntityUid uid, RCDComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !component.IsRpd || !args.Using.HasValue)
            return;

        // Only show when alt-clicking the RPD itself (args.Using is the held item)
        if (args.Using.Value != uid)
            return;

        var verb = new AlternativeVerb
        {
            Act = () => SwitchPipeMode(uid, component, args.User),
            Text = Loc.GetString("rcd-verb-switch-mode"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/settings.svg.192dpi.png")),
            Impact = LogImpact.Low
        };

        args.Verbs.Add(verb);
    }
    //DeltaV - RPD End
    private void OnAfterInteract(EntityUid uid, RCDComponent component, AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach)
            return;

        var user = args.User;
        var location = args.ClickLocation;

        var prototype = component.CachedPrototype; //DeltaV - RPD

        // Initial validity checks
        if (!location.IsValid(EntityManager))
            return;

        var gridUid = _transform.GetGrid(location);

        if (!TryComp<MapGridComponent>(gridUid, out var mapGrid))
        {
            _popup.PopupClient(Loc.GetString("rcd-component-no-valid-grid"), uid, user);
            return;
        }
        var tile = _mapSystem.GetTileRef(gridUid.Value, mapGrid, location);
        var position = _mapSystem.TileIndicesFor(gridUid.Value, mapGrid, location);
        //DeltaV - RPD Begin
        var layer = AtmosPipeLayer.Primary;
        if (component.IsRpd && prototype.HasLayers)
        {
            var tileSize = mapGrid.TileSize;
            var tileCenter = new Vector2(tile.X + tileSize / 2, tile.Y + tileSize / 2);
            var mouseCoordsDiff = args.ClickLocation.Position - tileCenter - new Vector2(0.5f, 0.5f);
            var mouseDeadzoneRadius = 0.25f;

            switch (component.CurrentMode)
            {
                case RpdMode.Primary:
                    layer = AtmosPipeLayer.Primary;
                    break;

                case RpdMode.Secondary:
                    layer = AtmosPipeLayer.Secondary;
                    break;

                case RpdMode.Tertiary:
                    layer = AtmosPipeLayer.Tertiary;
                    break;

                case RpdMode.Free:
                    // Only use mouse direction in Free mode
                    if (mouseCoordsDiff.Length() > mouseDeadzoneRadius && component.LastKnownEyeRotation.HasValue)
                    {
                        var gridRotation = _transform.GetWorldRotation(gridUid.Value);
                        var angle = new Angle(mouseCoordsDiff);
                        var eyeRotation = new Angle(component.LastKnownEyeRotation.Value);
                        var direction = (angle + eyeRotation + gridRotation + Math.PI / 2).GetCardinalDir();

                        layer = (direction == Direction.North || direction == Direction.East)
                            ? AtmosPipeLayer.Secondary
                            : AtmosPipeLayer.Tertiary;
                    }
                    break;
            }
        }
        //DeltaV - RPD End

        if (!IsRCDOperationStillValid(uid, component, gridUid.Value, mapGrid, tile, position, component.ConstructionDirection, args.Target, args.User))
            return;

        if (!_net.IsServer)
            return;

        // Get the starting cost, delay, and effect from the prototype
        var cost = prototype.Cost;
        var delay = prototype.Delay;
        var effectPrototype = prototype.Effect;

        #region: Operation modifiers

        // Deconstruction modifiers
        switch (prototype.Mode)
        {
            case RcdMode.Deconstruct:

                // Deconstructing an object
                if (args.Target != null)
                {
                    if (TryComp<RCDDeconstructableComponent>(args.Target, out var destructible))
                    {
                        cost = destructible.Cost;
                        delay = destructible.Delay;
                        effectPrototype = destructible.Effect;
                    }
                }

                // Deconstructing a tile
                else
                {
                    var deconstructedTile = _mapSystem.GetTileRef(gridUid.Value, mapGrid, location);
                    var protoName = !_turf.IsSpace(deconstructedTile) ? _deconstructTileProto : _deconstructLatticeProto;

                    if (_protoManager.Resolve(protoName, out var deconProto))
                    {
                        cost = deconProto.Cost;
                        delay = deconProto.Delay;
                        effectPrototype = deconProto.Effect;
                    }
                }

                break;

            case RcdMode.ConstructTile:

                // If replacing a tile, make the construction instant
                var contructedTile = _mapSystem.GetTileRef(gridUid.Value, mapGrid, location);

                if (!contructedTile.Tile.IsEmpty)
                {
                    delay = _instantConstructionDelay;
                    effectPrototype = _instantConstructionFx;
                }

                break;
        }

        #endregion

        // Try to start the do after
        var effect = Spawn(effectPrototype, location);
        var ev = new RCDDoAfterEvent(GetNetCoordinates(location), component.ConstructionDirection, component.ProtoId, cost, layer, GetNetEntity(effect)); //DeltaV - RPD

        var doAfterArgs = new DoAfterArgs(EntityManager, user, delay, ev, uid, target: args.Target, used: uid)
        {
            BreakOnDamage = true,
            BreakOnHandChange = true,
            BreakOnMove = true,
            AttemptFrequency = AttemptFrequency.EveryTick,
            CancelDuplicate = false,
            BlockDuplicate = false
        };

        args.Handled = true;

        if (!_doAfter.TryStartDoAfter(doAfterArgs))
            QueueDel(effect);
    }

    private void OnDoAfterAttempt(EntityUid uid, RCDComponent component, DoAfterAttemptEvent<RCDDoAfterEvent> args)
    {
        if (args.Event?.DoAfter?.Args == null)
            return;

        // Exit if the RCD prototype has changed
        if (component.ProtoId != args.Event.StartingProtoId)
        {
            args.Cancel();
            return;
        }

        // Ensure the RCD operation is still valid
        var location = GetCoordinates(args.Event.Location);

        var gridUid = _transform.GetGrid(location);

        if (!TryComp<MapGridComponent>(gridUid, out var mapGrid))
        {
            args.Cancel();
            return;
        }


        var tile = _mapSystem.GetTileRef(gridUid.Value, mapGrid, location);
        var position = _mapSystem.TileIndicesFor(gridUid.Value, mapGrid, location);

        if (!IsRCDOperationStillValid(uid, component, gridUid.Value, mapGrid, tile, position, args.Event.Direction, args.Event.Target, args.Event.User))
            args.Cancel();
    }

    private void OnDoAfter(EntityUid uid, RCDComponent component, RCDDoAfterEvent args)
    {
        if (args.Cancelled)
        {
            // Delete the effect entity if the do-after was cancelled (server-side only)
            if (_net.IsServer)
                QueueDel(GetEntity(args.Effect));
            return;
        }

        if (args.Handled)
            return;

        args.Handled = true;

        var location = GetCoordinates(args.Location);

        var gridUid = _transform.GetGrid(location);

        if (!TryComp<MapGridComponent>(gridUid, out var mapGrid))
            return;

        var tile = _mapSystem.GetTileRef(gridUid.Value, mapGrid, location);
        var position = _mapSystem.TileIndicesFor(gridUid.Value, mapGrid, location);

        // Ensure the RCD operation is still valid
        if (!IsRCDOperationStillValid(uid, component, gridUid.Value, mapGrid, tile, position, args.Direction, args.Target, args.User))
            return;

        // Finalize the operation (this should handle prediction properly)
        FinalizeRCDOperation(uid, component, gridUid.Value, mapGrid, tile, position, args.Direction, args.Target, args.User, args.Layer);

        // Play audio and consume charges
        _audio.PlayPredicted(component.SuccessSound, uid, args.User);
        _sharedCharges.AddCharges(uid, -args.Cost);
    }

    private void OnRCDconstructionGhostRotationEvent(RCDConstructionGhostRotationEvent ev, EntitySessionEventArgs session)
    {
        var uid = GetEntity(ev.NetEntity);

        // Determine if player that send the message is carrying the specified RCD in their active hand
        if (session.SenderSession.AttachedEntity is not { } player)
            return;

        if (_hands.GetActiveItem(player) != uid)
            return;

        if (!TryComp<RCDComponent>(uid, out var rcd))
            return;

        // Update the construction direction
        rcd.ConstructionDirection = ev.Direction;
        Dirty(uid, rcd);
    }
    //DeltaV - RPD Begin
    private void OnRCDConstructionGhostFlipEvent(RCDConstructionGhostFlipEvent ev, EntitySessionEventArgs session)
    {
        var uid = GetEntity(ev.NetEntity);

        if (session.SenderSession.AttachedEntity is not { } player)
            return;

        if (_hands.GetActiveItem(player) != uid)
            return;

        if (!TryComp<RCDComponent>(uid, out var rcd))
            return;

        rcd.UseMirrorPrototype = ev.UseMirrorPrototype;
        Dirty(uid, rcd);
    }

    private void SwitchPipeMode(EntityUid uid, RCDComponent component, EntityUid? user = null)
    {
        if (!component.IsRpd)
            return;

        // Cycle through modes
        component.CurrentMode = component.CurrentMode switch
        {
            RpdMode.Primary => RpdMode.Secondary,
            RpdMode.Secondary => RpdMode.Tertiary,
            RpdMode.Tertiary => RpdMode.Free,
            RpdMode.Free => RpdMode.Primary,
            _ => RpdMode.Free
        };

        Dirty(uid, component);

        if (user != null)
            _audio.PlayPredicted(component.SoundSwitchMode, uid, user.Value);
    }
    //DeltaV - RPD End
    #endregion

    #region Entity construction/deconstruction rule checks

    public bool IsRCDOperationStillValid(EntityUid uid, RCDComponent component, EntityUid gridUid, MapGridComponent mapGrid, TileRef tile, Vector2i position, EntityUid? target, EntityUid user, bool popMsgs = true)
    {
        return IsRCDOperationStillValid(uid, component, gridUid, mapGrid, tile, position, component.ConstructionDirection, target, user, popMsgs);
    }

    public bool IsRCDOperationStillValid(EntityUid uid, RCDComponent component, EntityUid gridUid, MapGridComponent mapGrid, TileRef tile, Vector2i position, Direction direction, EntityUid? target, EntityUid user, bool popMsgs = true)
    {
        // Update cached prototype if required
        UpdateCachedPrototype(uid, component); //DeltaV - RPD

        var prototype = component.CachedPrototype; //DeltaV - RPD

        // Check that the RCD has enough ammo to get the job done
        var charges = _sharedCharges.GetCurrentCharges(uid);

        // Both of these were messages were suppose to be predicted, but HasInsufficientCharges wasn't being checked on the client for some reason?
        if (charges == 0)
        {
            if (popMsgs)
                _popup.PopupClient(Loc.GetString("rcd-component-no-ammo-message"), uid, user);

            return false;
        }

        if (prototype.Cost > charges)
        {
            if (popMsgs)
                _popup.PopupClient(Loc.GetString("rcd-component-insufficient-ammo-message"), uid, user);

            return false;
        }

        // DeltaV - RPD Begin
        bool unobstructed;
        var worldPos = _mapSystem.GridTileToWorld(gridUid, mapGrid, position);

       if (component.IsRpd)
        {
            unobstructed = _interaction.InRangeUnobstructed(user, worldPos, 
                predicate: (entity) => !HasComp<PipeRestrictOverlapComponent>(entity), 
                popup: popMsgs);
        }
        else
        {
            unobstructed = (target == null)
                ? _interaction.InRangeUnobstructed(user, worldPos, popup: popMsgs)
                : _interaction.InRangeUnobstructed(user, target.Value, popup: popMsgs);
        }
        // DeltaV - RPD End

        if (!unobstructed)
            return false;

        // DeltaV - RPD Begin
        var layer = AtmosPipeLayer.Primary;
        if (component.IsRpd)
        {
            layer = GetLayerFromState(component, gridUid, mapGrid, tile, worldPos.Position);
        }
        // DeltaV - RPD End

        // Return whether the operation location is valid
        switch (prototype.Mode)
        {
            case RcdMode.ConstructTile:
            case RcdMode.ConstructObject:
                return IsConstructionLocationValid(uid, component, gridUid, mapGrid, tile, position, direction, user, layer, popMsgs); //DeltaV - RPD
            case RcdMode.Deconstruct:
                return IsDeconstructionStillValid(uid, component, tile, target, user, popMsgs); //DeltaV - RPD
        }

        return false;
    }

    // DeltaV - RPD Begin
    private AtmosPipeLayer GetLayerFromState(RCDComponent component, EntityUid gridUid, MapGridComponent mapGrid, TileRef tile, Vector2 clickPos)
    {
        if (!component.IsRpd || component.CachedPrototype == null || !component.CachedPrototype.HasLayers)
            return AtmosPipeLayer.Primary;

        var tileSize = mapGrid.TileSize;
        var tileCenter = new Vector2(tile.X + tileSize / 2f, tile.Y + tileSize / 2f);
        var mouseCoordsDiff = clickPos - tileCenter;
        var mouseDeadzoneRadius = 0.25f;

        switch (component.CurrentMode)
        {
            case RpdMode.Primary: return AtmosPipeLayer.Primary;
            case RpdMode.Secondary: return AtmosPipeLayer.Secondary;
            case RpdMode.Tertiary: return AtmosPipeLayer.Tertiary;
            case RpdMode.Free:
                if (mouseCoordsDiff.Length() > mouseDeadzoneRadius && component.LastKnownEyeRotation.HasValue)
                {
                    var gridRotation = _transform.GetWorldRotation(gridUid);
                    var angle = new Angle(mouseCoordsDiff);
                    var eyeRotation = new Angle(component.LastKnownEyeRotation.Value);
                    var direction = (angle + eyeRotation + gridRotation + Math.PI / 2).GetCardinalDir();
                    return (direction == Direction.North || direction == Direction.East) ? AtmosPipeLayer.Secondary : AtmosPipeLayer.Tertiary;
                }
                return AtmosPipeLayer.Primary;
            default: return AtmosPipeLayer.Primary;
        }
    }

    // DeltaV - RPD End

    private bool IsConstructionLocationValid(EntityUid uid, RCDComponent component, EntityUid gridUid, MapGridComponent mapGrid, TileRef tile, Vector2i position, Direction direction, EntityUid user, AtmosPipeLayer layer, bool popMsgs = true)
    {
        // Update cached prototype if required
        UpdateCachedPrototype(uid, component); //DeltaV - RPD

        var prototype = component.CachedPrototype; //DeltaV - RPD

        // Check rule: Must build on empty tile
        if (prototype.ConstructionRules.Contains(RcdConstructionRule.MustBuildOnEmptyTile) && !tile.Tile.IsEmpty)
        {
            if (popMsgs)
                _popup.PopupClient(Loc.GetString("rcd-component-must-build-on-empty-tile-message"), uid, user);

            return false;
        }

        // Check rule: Must build on non-empty tile
        if (!prototype.ConstructionRules.Contains(RcdConstructionRule.CanBuildOnEmptyTile) && tile.Tile.IsEmpty)
        {
            if (popMsgs)
                _popup.PopupClient(Loc.GetString("rcd-component-cannot-build-on-empty-tile-message"), uid, user);

            return false;
        }

        // Check rule: Must place on subfloor
        if (prototype.ConstructionRules.Contains(RcdConstructionRule.MustBuildOnSubfloor) && !_turf.GetContentTileDefinition(tile).IsSubFloor)
        {
            if (popMsgs)
                _popup.PopupClient(Loc.GetString("rcd-component-must-build-on-subfloor-message"), uid, user);

            return false;
        }

        // Tile specific rules
        if (prototype.Mode == RcdMode.ConstructTile)
        {
            // Check rule: Tile placement is valid
            if (!_floors.CanPlaceTile(gridUid, mapGrid, tile.GridIndices, out var reason))
            {
                if (popMsgs)
                    _popup.PopupClient(reason, uid, user);

                return false;
            }

            var tileDef = _turf.GetContentTileDefinition(tile);

            // Check rule: Respect baseTurf and baseWhitelist
            if (prototype.Prototype != null && _tileDefMan.TryGetDefinition(prototype.Prototype, out var replacementDef))
            {
                var replacementContentDef = (ContentTileDefinition) replacementDef;

                if (replacementContentDef.BaseTurf != tileDef.ID && !replacementContentDef.BaseWhitelist.Contains(tileDef.ID))
                {
                    if (popMsgs)
                        _popup.PopupClient(Loc.GetString("rcd-component-cannot-build-on-empty-tile-message"), uid, user);

                    return false;
                }
            }

            // Check rule: Tiles can't be identical
            if (tileDef.ID == prototype.Prototype)
            {
                if (popMsgs)
                    _popup.PopupClient(Loc.GetString("rcd-component-cannot-build-identical-tile"), uid, user);

                return false;
            }

            // Ensure that all construction rules shared between tiles and object are checked before exiting here
            return true;
        }

        // Entity specific rules

        // Check rule: The tile is unoccupied
        var isWindow = prototype.ConstructionRules.Contains(RcdConstructionRule.IsWindow);
        var isCatwalk = prototype.ConstructionRules.Contains(RcdConstructionRule.IsCatwalk);

        _intersectingEntities.Clear();
        _lookup.GetLocalEntitiesIntersecting(gridUid, position, _intersectingEntities, -0.05f, LookupFlags.Uncontained);

        foreach (var ent in _intersectingEntities)
        {
            // If the entity is the exact same prototype as what we are trying to build, then block it.
            // This is to prevent spamming objects on the same tile (e.g. lights)
            if (prototype.Prototype != null && MetaData(ent).EntityPrototype?.ID == prototype.Prototype)
            {
                // DeltaV - RPD Begin
                var isIdentical = true;

                if (component.IsRpd)
                {
                    // A pipe is only "identical" if it exists on the same layer we are trying to build on.
                    // We check the nodes of the existing entity to find its layer.
                    if (TryComp<NodeContainerComponent>(ent, out var nodeContainer))
                    {
                        var existingLayerMatch = false;
                        foreach (var node in nodeContainer.Nodes.Values)
                        {
                            if (node is IPipeNode pipeNode && pipeNode.Layer == layer)
                            {
                                existingLayerMatch = true;
                                break;
                            }
                        }

                        // If no nodes match the target layer, it's not identical (we can build here!)
                        if (!existingLayerMatch)
                            isIdentical = false;
                    }
                }
                
                else if (prototype.AllowMultiDirection)
                {
                    var entDirection = Transform(ent).LocalRotation.GetCardinalDir();
                    if (entDirection != direction)
                        isIdentical = false;
                }

                if (isIdentical)
                {
                    if (popMsgs) _popup.PopupClient(Loc.GetString("rcd-component-cannot-build-identical-entity"), uid, user);
                    return false;
                }
                // DeltaV - RPD End
            }

            if (isWindow && HasComp<SharedCanBuildWindowOnTopComponent>(ent))
                continue;

            if (isCatwalk && _tags.HasTag(ent, CatwalkTag))
            {
                if (popMsgs)
                    _popup.PopupClient(Loc.GetString("rcd-component-cannot-build-on-occupied-tile-message"), uid, user);

                return false;
            }

            // DeltaV - RPD Begin
            if (component.IsRpd && HasComp<PipeRestrictOverlapComponent>(ent))
            {
                continue; 
            }
            // DeltaV - RPD End

            if (prototype.CollisionMask != CollisionGroup.None && TryComp<FixturesComponent>(ent, out var fixtures))
            {
                foreach (var fixture in fixtures.Fixtures.Values)
                {
                    // Continue if no collision is possible
                    if (!fixture.Hard || fixture.CollisionLayer <= 0 || (fixture.CollisionLayer & (int) prototype.CollisionMask) == 0) 
                        continue;

                    // Continue if our custom collision bounds are not intersected
                    if (prototype.CollisionPolygon != null &&
                        !DoesCustomBoundsIntersectWithFixture(prototype.CollisionPolygon, component.ConstructionTransform, ent, fixture))
                        continue;

                    // Collision was detected
                    if (popMsgs)
                        _popup.PopupClient(Loc.GetString("rcd-component-cannot-build-on-occupied-tile-message"), uid, user);

                    return false;
                }
            }
        }

        return true;
    }

    private bool IsDeconstructionStillValid(EntityUid uid, RCDComponent component, TileRef tile, EntityUid? target, EntityUid user, bool popMsgs = true) //DeltaV - RPD
    {
        // Attempt to deconstruct a floor tile
        if (target == null)
        {
            //DeltaV - RPD Begin
            if (component.IsRpd)
            {
                if (popMsgs)
                    _popup.PopupClient(Loc.GetString("rcd-component-deconstruct-target-not-on-whitelist-message"), uid, user);

                return false;
            }
            //DeltaV - RPD End
            // The tile is empty
            if (tile.Tile.IsEmpty)
            {
                if (popMsgs)
                    _popup.PopupClient(Loc.GetString("rcd-component-nothing-to-deconstruct-message"), uid, user);

                return false;
            }

            // The tile has a structure sitting on it
            if (_turf.IsTileBlocked(tile, CollisionGroup.MobMask))
            {
                if (popMsgs)
                    _popup.PopupClient(Loc.GetString("rcd-component-tile-obstructed-message"), uid, user);

                return false;
            }

            // The tile cannot be destroyed
            var tileDef = _turf.GetContentTileDefinition(tile);

            if (tileDef.Indestructible)
            {
                if (popMsgs)
                    _popup.PopupClient(Loc.GetString("rcd-component-tile-indestructible-message"), uid, user);

                return false;
            }
        }

        // Attempt to deconstruct an object
        else
        {
            //DeltaV - RPD Begin
            // The object is not in the RPD whitelist
            if (!TryComp<RCDDeconstructableComponent>(target, out var deconstructible) || !deconstructible.RpdDeconstructable && component.IsRpd)
            {
                if (popMsgs)
                    _popup.PopupClient(Loc.GetString("rcd-component-deconstruct-target-not-on-whitelist-message"), uid, user);

                return false;
            }
            //DeltaV - RPD End

            // The object is not in the whitelist
            if (!deconstructible.Deconstructable) //DeltaV - RPD
            {
                if (popMsgs)
                    _popup.PopupClient(Loc.GetString("rcd-component-deconstruct-target-not-on-whitelist-message"), uid, user);

                return false;
            }

        }

        return true;
    }

    #endregion

    #region Entity construction/deconstruction

    private void FinalizeRCDOperation(EntityUid uid, RCDComponent component, EntityUid gridUid, MapGridComponent mapGrid, TileRef tile, Vector2i position, Direction direction, EntityUid? target, EntityUid user, AtmosPipeLayer layer)
    {
        if (!_net.IsServer)
            return;

        var prototype = component.CachedPrototype; //DeltaV - RPD

        if (prototype.Prototype == null)
            return;

        switch (prototype.Mode)
        {
            case RcdMode.ConstructTile:
                if (!_tileDefMan.TryGetDefinition(prototype.Prototype, out var tileDef))
                    return;

                _tile.ReplaceTile(tile, (ContentTileDefinition) tileDef, gridUid, mapGrid);
                _adminLogger.Add(LogType.RCD, LogImpact.High, $"{ToPrettyString(user):user} used RCD to set grid: {gridUid} {position} to {prototype.Prototype}");
                break;

            case RcdMode.ConstructObject:
            //DeltaV - RPD Begin
                var proto = (component.UseMirrorPrototype && !string.IsNullOrEmpty(prototype.MirrorPrototype))
                    ? prototype.MirrorPrototype
                    : prototype.Prototype;

                if (component.IsRpd && prototype.HasLayers)
                {
                    if (_protoManager.TryIndex<EntityPrototype>(proto, out var entityProto) &&
                        entityProto.TryGetComponent<AtmosPipeLayersComponent>(out var atmosPipeLayers, _entityManager.ComponentFactory) &&
                        _pipeLayersSystem.TryGetAlternativePrototype(atmosPipeLayers, layer, out var newProtoId))
                    {
                        proto = newProtoId;
                    }
                }

                // Calculate rotation before spawn
                var rotation = prototype.Rotation switch
                {
                    RcdRotation.Fixed => Angle.Zero,
                    RcdRotation.Camera => Transform(uid).LocalRotation,
                    RcdRotation.User => direction.ToAngle(),
                    _ => Angle.Zero // Fallback
                };

                // For RPD's, if overlapping existing pipe, replace the pipe
                if (component.IsRpd)
                {
                    // We need to know what the pipe *would* look like to check for overlaps
                    if (_protoManager.TryIndex<EntityPrototype>(proto, out var pipeProto) &&
                        pipeProto.TryGetComponent<NodeContainerComponent>(out var nodeContainer, _entityManager.ComponentFactory))
                    {
                        // Check every node in the prototype to see if it overlaps something on the grid
                        foreach (var node in nodeContainer.Nodes.Values)
                        {
                            if (node is IPipeNode pipeNode)
                            {
                                var proposed = new PipeRestrictOverlapSystem.ProposedPipe(
                                    pipeNode.Direction,
                                    layer,
                                    rotation
                                );

                                // If there is a conflict, delete the old pipe first
                                var conflict = _pipeOverlap.CheckIfWouldConflict(gridUid, position, proposed);
                                if (Exists(conflict) && HasComp<RCDDeconstructableComponent>(conflict))
                                {
                                    _adminLogger.Add(LogType.RCD, LogImpact.Medium,
                                        $"{ToPrettyString(user):user} RPD replaced {ToPrettyString(conflict.Value)} at {position}");
                                    Del(conflict.Value);
                                    _audio.PlayPvs(component.SuccessSound, uid);
                                }
                            }
                        }
                    }
                }

                var entityCoords = _mapSystem.GridTileToLocal(gridUid, mapGrid, position);
                var mapCoords = new MapCoordinates(entityCoords.ToMapPos(EntityManager, _transform), entityCoords.GetMapId(EntityManager));

                var ent = Spawn(proto, mapCoords, rotation: rotation);
                //DeltaV - RPD End
                switch (prototype.Rotation)
                {
                    case RcdRotation.Fixed:
                        Transform(ent).LocalRotation = Angle.Zero;
                        break;
                    case RcdRotation.Camera:
                        Transform(ent).LocalRotation = Transform(uid).LocalRotation;
                        break;
                    case RcdRotation.User:
                        Transform(ent).LocalRotation = direction.ToAngle();
                        break;
                }

                _adminLogger.Add(LogType.RCD, LogImpact.High, $"{ToPrettyString(user):user} used RCD to spawn {ToPrettyString(ent)} at {position} on grid {gridUid}");
                break;

            case RcdMode.Deconstruct:

                if (target == null)
                {
                    // Deconstruct tile, don't drop tile as item
                    if (_tile.DeconstructTile(tile, spawnItem: false))
                        _adminLogger.Add(LogType.RCD, LogImpact.High, $"{ToPrettyString(user):user} used RCD to set grid: {gridUid} tile: {position} open to space");
                }
                else
                {
                    // Deconstruct object
                    _adminLogger.Add(LogType.RCD, LogImpact.High, $"{ToPrettyString(user):user} used RCD to delete {ToPrettyString(target):target}");
                    QueueDel(target);
                }

                break;
        }
    }

    #endregion

    #region Utility functions

    private bool DoesCustomBoundsIntersectWithFixture(PolygonShape boundingPolygon, Transform boundingTransform, EntityUid fixtureOwner, Fixture fixture)
    {
        var entXformComp = Transform(fixtureOwner);
        var entXform = new Transform(new(), entXformComp.LocalRotation);

        return boundingPolygon.ComputeAABB(boundingTransform, 0).Intersects(fixture.Shape.ComputeAABB(entXform, 0));
    }
    //DeltaV - RPD Begin
    public void UpdateCachedPrototype(EntityUid uid, RCDComponent component)
    {
        if (component.ProtoId.Id != component.CachedPrototype?.Prototype ||
            (component.CachedPrototype?.MirrorPrototype != null &&
             component.ProtoId.Id != component.CachedPrototype?.MirrorPrototype))
        {
            component.CachedPrototype = _protoManager.Index(component.ProtoId);
        }
    }

    public RpdMode GetCurrentRpdMode(EntityUid uid, RCDComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return RpdMode.Free; // default to Free mode

        return component.CurrentMode;
    }
    //DeltaV - RPD End
    #endregion
}

[Serializable, NetSerializable]
public sealed partial class RCDDoAfterEvent : DoAfterEvent
{
    [DataField(required: true)]
    public NetCoordinates Location { get; private set; }

    [DataField]
    public Direction Direction { get; private set; }

    [DataField]
    public ProtoId<RCDPrototype> StartingProtoId { get; private set; }

    [DataField]
    public int Cost { get; private set; } = 1;

    [DataField("fx")]
    public NetEntity? Effect { get; private set; }

    [DataField] //DeltaV - RPD
    public AtmosPipeLayer Layer { get; private set; } = AtmosPipeLayer.Primary; //DeltaV - RPD

    private RCDDoAfterEvent() { }

    public RCDDoAfterEvent(NetCoordinates location, Direction direction, ProtoId<RCDPrototype> startingProtoId, int cost, AtmosPipeLayer layer, NetEntity? effect = null)
    {
        Location = location;
        Direction = direction;
        StartingProtoId = startingProtoId;
        Cost = cost;
        Layer = layer; //DeltaV - RPD
        Effect = effect;
    }

    public override DoAfterEvent Clone()
    {
        return this;
    }
}
