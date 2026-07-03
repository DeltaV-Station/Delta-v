using Content.Server.Mind;
using Content.Server.Zombies;
using Content.Shared._DV.Diona;
using Content.Shared.Body;
using Content.Shared.Gibbing;
using Content.Shared.Species.Components;
using Content.Shared.Zombies;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Diona;

public sealed class NymphSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly ZombieSystem _zombie = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DVNymphingOrganComponent, BodyRelayedEvent<BeingGibbedEvent>>(OnBeingGibbed);
    }

    private void OnBeingGibbed(Entity<DVNymphingOrganComponent> ent, ref BodyRelayedEvent<BeingGibbedEvent> args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        if (!_protoManager.TryIndex<EntityPrototype>(ent.Comp.EntityPrototype, out var entityProto))
            return;

        // Get the organs' position & spawn a nymph there
        var coords = Transform(ent).Coordinates;
        var nymph = SpawnAtPosition(entityProto.ID, coords);

        if (HasComp<ZombieComponent>(args.Body)) // Zombify the new nymph if old one is a zombie
            _zombie.ZombifyEntity(nymph);

        // Move the mind if there is one and it's supposed to be transferred
        if (ent.Comp.TransferMind && _mindSystem.TryGetMind(args.Body, out var mindId, out var mind))
            _mindSystem.TransferTo(mindId, nymph, true, mind: mind);

        // Delete the old organ
        QueueDel(ent);
        args.Args.Giblets.Add(nymph);
    }
}
