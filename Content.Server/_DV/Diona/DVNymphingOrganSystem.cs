using Content.Server.Mind;
using Content.Server.Zombies;
using Content.Shared._DV.Diona;
using Content.Shared.Body;
using Content.Shared.Gibbing;
using Content.Shared.Humanoid;
using Content.Shared.NameIdentifier;
using Content.Shared.NameModifier.EntitySystems;
using Content.Shared.Species.Components;
using Content.Shared.Zombies;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Diona;

public sealed class NymphingOrganSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly ZombieSystem _zombie = default!;
    [Dependency] private readonly SharedVisualBodySystem _visualBody = default!;
    [Dependency] private readonly NameModifierSystem _nameModifier = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DVNymphingOrganComponent, BodyRelayedEvent<BeingGibbedEvent>>(OnBeingGibbed);
        SubscribeLocalEvent<DVNymphProfileComponent, RefreshNameModifiersEvent>(OnRefreshNameModifiers);
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

        if (TryComp<DVNymphProfileComponent>(nymph, out var nymphProfile))
        {
            nymphProfile.Name = Name(args.Body);

            if (TryComp<HumanoidProfileComponent>(args.Body, out var bodyProfile))
            {
                nymphProfile.Species = bodyProfile.Species;
                nymphProfile.Gender = bodyProfile.Gender;
                nymphProfile.Sex = bodyProfile.Sex;
                nymphProfile.Age = bodyProfile.Age;
                nymphProfile.Height = bodyProfile.Height;
            }

            if (_visualBody.TryGatherMarkingsData(args.Body.Owner, null, out var profiles, out _, out var applied))
            {
                nymphProfile.OrganMarkings = applied;
                nymphProfile.OrganProfiles = profiles;
            }

            Dirty(nymph, nymphProfile);
            _nameModifier.RefreshNameModifiers(nymph);
        }

        // Delete the old organ
        QueueDel(ent);
        args.Args.Giblets.Add(nymph);
    }

    private void OnRefreshNameModifiers(Entity<DVNymphProfileComponent> ent, ref RefreshNameModifiersEvent args)
    {
        if (ent.Comp.Name is { } name)
            args.AddModifier("nymph-name-prefix", 0, ("identityName", name));
    }
}
