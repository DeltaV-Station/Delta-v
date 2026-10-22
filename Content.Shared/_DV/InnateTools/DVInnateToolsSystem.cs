using System.Linq;
using Content.Shared.EntityTable;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Components;

namespace Content.Shared._DV.InnateTools;

public sealed class DVInnateToolsSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly EntityTableSystem _entityTable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DVInnateToolsComponent, MapInitEvent>(OnInnateToolsInit);
        SubscribeLocalEvent<DVInnateToolsComponent, ComponentShutdown>(OnInnateToolsShutdown);
    }

    private void OnInnateToolsInit(Entity<DVInnateToolsComponent> ent, ref MapInitEvent args)
    {
        var items = _entityTable.GetSpawns(ent.Comp.Tools).ToList();
        var xform = Transform(ent);

        for (var i = 0; i < items.Count; i++)
        {
            var name = $"{GetNetEntity(ent)}-innate-{i}";
            _hands.AddHand(ent.Owner,
                name,
                i switch
            {
                0 => HandLocation.Right,
                _ when i < items.Count-1 => HandLocation.Middle,
                _ => HandLocation.Left,
            });

            var spawned = PredictedSpawnAtPosition(items[i], xform.Coordinates);
            _hands.DoPickup(ent.Owner, name, spawned);

            EnsureComp<UnremoveableComponent>(spawned);
        }

        ent.Comp.Provided = items.Count;
        Dirty(ent);
    }

    private void OnInnateToolsShutdown(Entity<DVInnateToolsComponent> ent, ref ComponentShutdown args)
    {
        for (var i = 0; i < ent.Comp.Provided; i++)
        {
            var name = $"{GetNetEntity(ent)}-innate-{i}";
            if (_hands.TryGetHeldItem(ent.Owner, name, out var held))
                QueueDel(held);
        }
    }
}
