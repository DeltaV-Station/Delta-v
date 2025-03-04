using Content.Server._DV.Mapping;
using Robust.Shared.ContentPack;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Linq;

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
        var mapLoader = entMan.System<MapLoaderSystem>();
        var catSys = entMan.System<MappingCategoriesSystem>(); // meow

        await server.WaitPost(() =>
        {
            Assert.Multiple(() =>
            {
                var mapFolder = new ResPath(MapsPath);
                foreach (var map in resMan.ContentFindFiles(mapFolder))
                {
                    if (map.Extension != "yml" || map.Filename.StartsWith(".", StringComparison.Ordinal))
                        continue;

                    var rootedPath = map.ToRootedPath().ToString();
                    if (rootedPath.StartsWith(TestMapsPath, StringComparison.Ordinal))
                        continue;

                    Assert.That(mapLoader.TryLoadGeneric(map, out var maps, out _), $"Failed to load map {rootedPath}");
                    Assert.That(maps.Count, Is.EqualTo(1), $"Map {rootedPath} had multiple maps serialized!");

                    var allowed = catSys.GetAllowedCategories(rootedPath);
                    var query = entMan.EntityQueryEnumerator<MappingCategoriesComponent>();
                    while (query.MoveNext(out var uid, out var comp))
                    {
                        var ent = (uid, comp);
                        Assert.That(catSys.CanMap(ent, allowed), $"Entity {entMan.ToPrettyString(uid)} cannot be mapped on {rootedPath}");
                    }

                    entMan.DeleteEntity(maps.First());
                }
            });
        });

        await server.WaitRunTicks(1);

        await pair.CleanReturnAsync();
    }
}
