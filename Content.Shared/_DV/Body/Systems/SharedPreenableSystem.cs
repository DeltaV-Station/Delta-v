using Content.Shared._DV.Body.Components;
using Content.Shared._DV.Body.Events;
using Content.Shared._DV.Humanoid;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared.Random.Helpers;
using Content.Shared.Verbs;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._DV.Body.Systems;

public sealed class PreenableSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PreenableComponent, GetVerbsEvent<Verb>>(AddVerb);
        SubscribeLocalEvent<PreenableComponent, PreeningEvent>(OnPreened);
        SubscribeLocalEvent<PreenableComponent, DamageChangedEvent>(OnDamaged);
    }

    private void AddVerb(Entity<PreenableComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        if (!args.CanInteract)
            return;

        // can't preen with no feathers
        if (ent.Comp.CurrentFeathers <= 0)
            return;

        var user = args.User;

        Verb verb = new()
        {
            Act = () => AttemptDoAfter(ent, user),
            Text = "Preen Feathers",
        };
        args.Verbs.Add(verb);
    }

    private void AttemptDoAfter(Entity<PreenableComponent> ent, EntityUid userUid)
    {
        var doArgs = new DoAfterArgs(EntityManager, userUid, 5f, new PreeningEvent(), ent, ent)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
        };

        _doAfter.TryStartDoAfter(doArgs);
    }

    private void OnPreened(Entity<PreenableComponent> ent, ref PreeningEvent args)
    {
        var feather = SpawnFeather(ent);

        _hands.TryPickupAnyHand(args.User, feather);
    }

    private void OnDamaged(Entity<PreenableComponent> ent, ref DamageChangedEvent args)
    {
        if (args.DamageDelta == null || ent.Comp.ValidDamageGroups == null)
            return;

        var totalApplicableDamage = FixedPoint2.Zero;
        foreach (var (group, value) in args.DamageDelta.GetDamagePerGroup(_prototype))
        {
            if (!ent.Comp.ValidDamageGroups.Contains(group))
                continue;

            totalApplicableDamage += value;
        }

        // We do not have predicted random from upstream and it's MESSING ME UP!!
        // once we DO get it, make this a random chance


    }

    private EntityUid SpawnFeather(Entity<PreenableComponent> ent)
    {
        var feather = PredictedSpawnAtPosition(ent.Comp.FeatherPrototype.Id, Transform(ent).Coordinates);

        if (TryComp<HumanoidAppearanceComponent>(ent, out var appearance))
        {
            _appearance.SetData(feather, FeatherVisuals.FeatherColor, appearance.SkinColor);
        }

        return feather;
    }
}
