using Content.Server._DV.Mapping;
using Robust.Shared.ContentPack;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Log;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Utility;
using System.Collections.Generic;
using System.Linq;

namespace Content.IntegrationTests.Tests._DV;

/// <summary>
/// Checks that every mapped entity with <see cref="MappingCategoriesComponent"/> is allowed to be mapped.
/// </summary>
public sealed class MappingCategoryTest
{
    private const string MapsPath = "/Maps";
    // dev map doesn't matter and don't want to change it
    private readonly List<string> _ignoredMapsPath = ["/Maps/Test/", "/Maps/Shuttles/AdminSpawn"];

    [Test]
    public async Task NonGameMapsLoadableTest()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();
        var resMan = server.ResolveDependency<IResourceManager>();
        var mapLoader = entMan.System<MapLoaderSystem>();
        var catSys = entMan.System<MappingCategoriesSystem>(); // meow
        var mapSys = entMan.System<SharedMapSystem>();
        var sawmill = server.ResolveDependency<ILogManager>().GetSawmill("mapping_categories");

        await server.WaitPost(() =>
        {
            Assert.Multiple(() =>
            {
                var mapFolder = new ResPath(MapsPath);
                var allMaps = resMan.ContentFindFiles(mapFolder);

                // Filter out paths we don't care about
                foreach (var testMapPath in _ignoredMapsPath)
                {
                    allMaps = allMaps.Where(x => !x.ToRootedPath().ToString().StartsWith(testMapPath));
                }

                foreach (var map in allMaps)
                {
                    if (map.Extension != "yml" || map.Filename.StartsWith(".", StringComparison.Ordinal))
                        continue;

                    var rootedPath = map.ToRootedPath().ToString();
                    if (GetCategory(map, mapLoader) is not {} category)
                    {
                        sawmill.Warning($"Map {map} is missing a category, skipping it.");
                        continue;
                    }

                    var mapUid = mapSys.CreateMap(out var mapId);
                    var opts = new MapLoadOptions
                    {
                        MergeMap = category == FileCategory.Map
                            ? null // don't try to reparent maps
                            : mapId // needed or else grids will be de-orphaned which is bad
                    };
                    Assert.That(mapLoader.TryLoadGeneric(map, out var maps, out _, opts), $"Failed to load map {rootedPath}");
                    maps.Add(mapUid);

                    var allowed = catSys.GetAllowedCategories(rootedPath);
                    var query = entMan.EntityQueryEnumerator<MappingCategoriesComponent>();
                    while (query.MoveNext(out var uid, out var comp))
                    {
                        var ent = (uid, comp);
                        Assert.That(catSys.CanMap(ent, allowed), $"Entity {entMan.ToPrettyString(uid)} cannot be mapped on {rootedPath}");
                    }

                    foreach (var uid in maps)
                    {
                        entMan.DeleteEntity(uid);
                    }
                }
            });
        });

        await server.WaitRunTicks(1);

        await pair.CleanReturnAsync();
    }

    // me when engine doesnt have this
    private FileCategory? GetCategory(ResPath path, MapLoaderSystem mapLoader)
    {
        Assert.That(mapLoader.TryReadFile(path, out var data), $"Failed to read map file {path}");
        var meta = data.Get<MappingDataNode>("meta");
        if (!meta.TryGet<ValueDataNode>("category", out var node))
            return null;

        var valid = Enum.TryParse<FileCategory>(node.Value, out var cat);
        Assert.That(valid, $"Category for {path} is invalid");
        return cat;
    }
}
