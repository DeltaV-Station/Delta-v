using Content.Shared._DV.Body.Components;
using Content.Shared._DV.Body.Events;
using Content.Shared.Body.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Forensics.Systems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
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
    [Dependency] private readonly SharedForensicsSystem _forensics = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;

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
            Text = Loc.GetString(ent.Comp.PreeningVerbString),
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

        if (userUid == ent.Owner)
        {
            _popup.PopupClient(Loc.GetString(ent.Comp.SelfPreeningMessage), ent, ent);
        }
        else
        {
            _popup.PopupEntity(Loc.GetString(ent.Comp.GettingPreenedMessage, ("preener", Identity.Entity(userUid, EntityManager))), ent, ent, PopupType.Medium);
            _popup.PopupClient(Loc.GetString(ent.Comp.PreeningOtherMessage, ("preenee", Identity.Entity(ent, EntityManager))), userUid, userUid);
        }

        _doAfter.TryStartDoAfter(doArgs);
    }

    private void OnPreened(Entity<PreenableComponent> ent, ref PreeningEvent args)
    {
        if (args.Cancelled)
            return;

        if (ent.Comp.CurrentFeathers <= 0)
            return;

        var feather = SpawnFeather(ent, false);

        _hands.TryPickupAnyHand(args.User, feather);
    }

    private void OnDamaged(Entity<PreenableComponent> ent, ref DamageChangedEvent args)
    {
        if (args.DamageDelta == null || ent.Comp.ValidDamageGroups == null)
            return;

        if (ent.Comp.CurrentFeathers <= 0)
            return;

        var totalApplicableDamage = FixedPoint2.Zero;
        foreach (var (group, value) in args.DamageDelta.GetDamagePerGroup(_prototype))
        {
            if (!ent.Comp.ValidDamageGroups.Contains(group))
                continue;

            totalApplicableDamage += value;
        }

        if (totalApplicableDamage <= ent.Comp.ShedDamageThreshold)
            return;

        // predicted randomness is a truly evil thing
        var rand = SharedRandomExtensions.PredictedRandom(_timing, GetNetEntity(ent));
        var randomDouble = rand.NextDouble();

        var triggerChance = totalApplicableDamage * ent.Comp.ShedScalingChance;

        if (randomDouble >= triggerChance)
            return;

        var feather = SpawnFeather(ent, true);

        // apply a random impulse so it's flying off the body. similar code to GibbingSystem
        var scatterVector = rand.NextAngle().ToVec() * (rand.NextFloat(10, 40));

        // update name/desc for increased validness
        var meta = MetaData(feather);
        _metaData.SetEntityName(feather, Loc.GetString(ent.Comp.FeatherBloodiedNameString, ("item", Name(feather))), meta);
        _metaData.SetEntityDescription(feather, Loc.GetString(ent.Comp.FeatherBloodiedDescString), meta);
        Dirty(feather, meta);

        _physics.ApplyLinearImpulse(feather, scatterVector);
        _physics.ApplyAngularImpulse(feather, rand.NextFloat(-30, 30));
    }

    private EntityUid SpawnFeather(Entity<PreenableComponent> ent, bool bloody)
    {
        var feather = PredictedSpawnAtPosition(ent.Comp.FeatherPrototype.Id, Transform(ent).Coordinates);

        if (TryComp<HumanoidAppearanceComponent>(ent, out var appearance))
        {
            _appearance.SetData(feather, FeatherVisuals.FeatherColor, appearance.SkinColor);
        }

        // best be careful, no cleaning this
        _forensics.TransferDna(feather, ent, false);

        ent.Comp.CurrentFeathers -= 1;
        ent.Comp.ReplenishTime = _timing.CurTime + ent.Comp.ReplenishDelay;
        Dirty(ent);

        if (!bloody || !TryComp<BloodstreamComponent>(ent, out var bloodstream) || bloodstream.BloodSolution == null)
            return feather;

        var solution = bloodstream.BloodSolution.Value.Comp.Solution;
        _appearance.SetData(feather, FeatherVisuals.BloodColor, solution.GetColor(_prototype));

        return feather;
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        var preenableQuery = EntityQueryEnumerator<PreenableComponent>();

        while (preenableQuery.MoveNext(out var uid, out var preenable))
        {
            if (preenable.ReplenishTime == null || !(preenable.ReplenishTime <= _timing.CurTime))
                continue;

            if (preenable.CurrentFeathers >= preenable.MaximumFeathers)
                continue;

            preenable.CurrentFeathers += 1;

            if (preenable.CurrentFeathers >= preenable.MaximumFeathers)
            {
                preenable.ReplenishTime = null;
                continue;
            }

            preenable.ReplenishTime = _timing.CurTime + preenable.ReplenishDelay;
        }
    }
}
