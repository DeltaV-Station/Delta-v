using Content.Server._DV.Mapping;
using Robust.Server.GameObjects;
using Robust.Shared.ContentPack;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.IntegrationTests.Tests._DV;

/// <summary>
/// Checks that every mapped entity with <see cref="MappingCategoriesComponent"/> is allowed to be mapped.
/// </summary>
public sealed class MappingCategoryTest
{
    private const string MapsPath = "/Maps";
    // dev map doesn't matter and don't want to change it
    private const string TestMapsPath = "/Maps/Test/";

    [Test]
    public async Task NonGameMapsLoadableTest()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();
        var resMan = server.ResolveDependency<IResourceManager>();
        var mapMan = server.ResolveDependency<IMapManager>();
        var mapLoader = entMan.System<MapLoaderSystem>();
        var mapSys = entMan.System<SharedMapSystem>();
        var catSys = entMan.System<MappingCategoriesSystem>(); // meow

        await server.WaitPost(() =>
        {
            var mapFolder = new ResPath(MapsPath);
            foreach (var map in resMan.ContentFindFiles(mapFolder))
            {
                if (map.Extension != "yml" || map.Filename.StartsWith(".", StringComparison.Ordinal))
                    continue;

                var rootedPath = map.ToRootedPath().ToString();
                if (rootedPath.StartsWith(TestMapsPath, StringComparison.Ordinal))
                    continue;

                mapSys.CreateMap(out var mapId);
                Assert.That(mapLoader.TryLoad(mapId, rootedPath, out var roots), "Failed to load map {rootedPath}");

                var allowed = catSys.GetAllowedCategories(rootedPath);
                var query = entMan.EntityQueryEnumerator<MappingCategoriesComponent>();
                Assert.Multiple(() =>
                {
                    while (query.MoveNext(out var uid, out var comp))
                    {
                        var ent = (uid, comp);
                        Assert.That(catSys.CanMap(ent, allowed), "Entity {entMan.ToPrettyString(uid)} cannot be mapped on {rootedPath}");
                    }
                });

                mapMan.DeleteMap(mapId);
            }
        });

        await server.WaitRunTicks(1);

        await pair.CleanReturnAsync();
    }
}
