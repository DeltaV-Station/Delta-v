using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Maps;
using Content.Shared.Tag;
using Robust.Shared.Console;
using Robust.Shared.Map;
using Robust.Server.GameObjects;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.Construction.Commands;

[AdminCommand(AdminFlags.Mapping)]
public sealed class TileWallsCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefManager = default!;

    // ReSharper disable once StringLiteralTypo
    public string Command => "tilewalls";
    public string Description => "Puts an underplating tile below every wall on a grid.";
    public string Help => $"Usage: {Command} <gridId> | {Command}";

    public static readonly ProtoId<ContentTileDefinition> TilePrototypeId = "Plating";
    public static readonly ProtoId<ContentTileDefinition> TilePrototypeId2 = "FloorCave"; // Delta V - Add cave floor under asteroid rocks
    public static readonly ProtoId<TagPrototype> WallTag = "Wall";
    public static readonly ProtoId<TagPrototype> NaturalTag = "Natural"; // Delta V - Add cave floor under asteroid rocks
    public static readonly ProtoId<TagPrototype> WoodenTag = "Wooden"; // Delta V - Ignore wooden support walls
    public static readonly ProtoId<TagPrototype> DiagonalTag = "Diagonal";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var player = shell.Player;
        EntityUid? gridId;

        switch (args.Length)
        {
            case 0:
                if (player?.AttachedEntity is not { Valid: true } playerEntity)
                {
                    shell.WriteError("Only a player can run this command.");
                    return;
                }

                gridId = _entManager.GetComponent<TransformComponent>(playerEntity).GridUid;
                break;
            case 1:
                if (!NetEntity.TryParse(args[0], out var idNet) || !_entManager.TryGetEntity(idNet, out var id))
                {
                    shell.WriteError($"{args[0]} is not a valid entity.");
                    return;
                }

                gridId = id;
                break;
            default:
                shell.WriteLine(Help);
                return;
        }

        if (!_entManager.TryGetComponent(gridId, out MapGridComponent? grid))
        {
            shell.WriteError($"No grid exists with id {gridId}");
            return;
        }

        if (!_entManager.EntityExists(gridId))
        {
            shell.WriteError($"Grid {gridId} doesn't have an associated grid entity.");
            return;
        }

        var tagSystem = _entManager.EntitySysManager.GetEntitySystem<TagSystem>();
        var underplating = _tileDefManager[TilePrototypeId];
        var underplatingTile = new Tile(underplating.TileId);
        var naturalunderplating = _tileDefManager[TilePrototypeId2]; // Delta V - Add cave floor under asteroid rocks
        var naturalunderplatingTile = new Tile(naturalunderplating.TileId); // Delta V - Add cave floor under asteroid rocks
        var changed = 0;
        var enumerator = _entManager.GetComponent<TransformComponent>(gridId.Value).ChildEnumerator;
        while (enumerator.MoveNext(out var child))
        {
            if (!_entManager.EntityExists(child))
            {
                continue;
            }

            if (!tagSystem.HasTag(child, WallTag))
            {
                continue;
            }

            if (tagSystem.HasTag(child, DiagonalTag) || tagSystem.HasTag(child, WoodenTag)) // Delta V - Ignore wooden support walls
            {
                continue;
            }

            var childTransform = _entManager.GetComponent<TransformComponent>(child);

            if (!childTransform.Anchored)
            {
                continue;
            }

            var mapSystem = _entManager.System<MapSystem>();
            var tile = mapSystem.GetTileRef(gridId.Value, grid, childTransform.Coordinates);
            var tileDef = (ContentTileDefinition)_tileDefManager[tile.Tile.TypeId];
            // Delta V - Begin add natural wall tile replace
            if (tileDef.ID == TilePrototypeId && !tagSystem.HasTag(child, NaturalTag))
            {
                continue;
            }

            if (tagSystem.HasTag(child, NaturalTag))
            {
                if (tileDef.ID == TilePrototypeId2)
                {
                    continue;
                }
                mapSystem.SetTile(gridId.Value, grid, childTransform.Coordinates, naturalunderplatingTile);
                changed++;
                continue;
            }
            // Delta V - end add natural wall tile replace
            mapSystem.SetTile(gridId.Value, grid, childTransform.Coordinates, underplatingTile);
            changed++;
        }

        shell.WriteLine($"Changed {changed} tiles.");
    }
}
