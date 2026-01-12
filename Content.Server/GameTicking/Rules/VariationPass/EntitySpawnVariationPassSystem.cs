using System.Linq;
using Content.Server.GameTicking.Rules.VariationPass.Components;
using Content.Shared.Storage;
using Content.Shared.Tag;
using Robust.Shared.Random;

namespace Content.Server.GameTicking.Rules.VariationPass;

/// <inheritdoc cref="EntitySpawnVariationPassComponent"/>
public sealed class EntitySpawnVariationPassSystem : VariationPassSystem<EntitySpawnVariationPassComponent>
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!; // imp edit
    [Dependency] private readonly TagSystem _tags = default!;

    protected override void ApplyVariation(Entity<EntitySpawnVariationPassComponent> ent, ref StationVariationPassEvent args)
    {
        var totalTiles = Stations.GetTileCount(args.Station.AsNullable());

        var dirtyMod = Random.NextGaussian(ent.Comp.TilesPerEntityAverage, ent.Comp.TilesPerEntityStdDev);
        var trashTiles = Math.Max((int) (totalTiles * (1 / dirtyMod)), 0);

        for (var i = 0; i < trashTiles; i++)
        {
            if (!TryFindRandomTileOnStation(args.Station, out _, out _, out var coords))
                continue;

            // Delta-V: Entity spawn variation tag blacklist
            var valid = true;

            if (ent.Comp.TagsBlacklist != null && ent.Comp.TagsBlacklist.Length > 0)
            {
                foreach (var otherEnt in _lookup.GetEntitiesIntersecting(coords))
                {
                    if (!_tags.HasAnyTag(otherEnt, ent.Comp.TagsBlacklist))
                        continue;
                    
                    valid = false;
                    break;
                }
            }

            if (!valid)
                continue;
            // Delta-V

            var ents = EntitySpawnCollection.GetSpawns(ent.Comp.Entities, Random);
            foreach (var spawn in ents)
            {
                SpawnAtPosition(spawn, coords);
            }
        }
    }
}
