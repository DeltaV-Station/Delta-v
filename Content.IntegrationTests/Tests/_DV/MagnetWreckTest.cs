using Content.IntegrationTests.Fixtures;
using Content.Shared.Procedural;
using Content.Shared.Tag;
using Robust.Server.GameObjects;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.GameObjects;
using Robust.Shared.Log;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;


namespace Content.IntegrationTests.Tests._DV;

/// <summary>
/// Checks that maps templated for asteroid or debris spawn are correctly configured.
/// </summary>
public sealed class MagnetWreckTest : GameTest
{
    [Test]
    public async Task ValidateWreckMaps()
    {
        var pair = Pair;
        var server = pair.Server;
        var mapManager = server.ResolveDependency<IMapManager>();
        var protoMan = server.ProtoMan;
        var entMan = server.ResolveDependency<IEntityManager>();
        var mapLoader = entMan.System<MapLoaderSystem>();
        var mapSys = server.System<MapSystem>();


        var logger = Client.ResolveDependency<ILogManager>().RootSawmill;

        ProtoId<TagPrototype> asteroidWreckTag = "AsteroidWreck";
        ProtoId<TagPrototype> debrisWreckTag = "DebrisWreck";

        await server.WaitPost(() =>
        {
            Assert.Multiple(() =>
            {
                foreach (var proto in protoMan.EnumeratePrototypes<DungeonRoomPrototype>())
                {
                    if (!proto.Tags.Contains(asteroidWreckTag) && !proto.Tags.Contains(debrisWreckTag))
                        continue;

                    //If any other issues have crept into maps that would cause an error, it is useful to know what map we're looking at!
                    logger.Debug($"Testing map {proto.ID}");

                    //It helps to know if the map will load in the first place.
                    Assert.That(mapLoader.TryLoadMap(proto.AtlasPath, out var map, out _),
                        $"Failed to load map on {proto.ID}");
                    var wreckMap = map!.Value.Comp.MapId;
                    var wreckMapUid = mapSys.GetMapOrInvalid(wreckMap);
                    var wreckGrid = entMan.GetComponent<MapGridComponent>(wreckMapUid);

                    for (var x = 0; x < proto.Size.X; x++)
                    {
                        for (var y = 0; y < proto.Size.Y; y++)
                        {
                            var indices = new Vector2i(x + proto.Offset.X, y + proto.Offset.Y);
                            var tileRef = mapSys.GetTileRef(wreckMapUid, wreckGrid, indices);

                            //Ensure that the mapped space fills the prototype size/offset, and that every grid coord has a floortile. The DunGenRoom system gets exciting if these do not apply.
                            Assert.That(!Equals(tileRef.Tile, default(Tile)),
                                $"Wreck {proto.ID} contains empty space at {indices.X},{indices.Y}. Check the size & offset, and that the full grid has floor tiles. Use ignoreTile for empty space & parent overlap.");
                        }
                    }

                    entMan.DeleteEntity(wreckMapUid);
                }
            });
        });
    }
}
