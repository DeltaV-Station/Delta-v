using Content.Server.Actions;
using Content.Server.Damage.Systems;
using Content.Server.Hands.Systems;
using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Server.Storage.EntitySystems;
using Content.Shared._DV.BloodDraining.Events;
using Content.Shared._DV.Vampires.Components;
using Content.Shared._DV.Vampires.EntitySystems;
using Content.Shared._DV.Vampires.Events;
using Content.Shared.Damage;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Mobs;
using Content.Shared.Polymorph;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;


namespace Content.Server._DV.Vampires.EntitySystems;

public sealed class VampireSystem : SharedVampireSystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly EntityStorageSystem _entityStorageSystem = default!;
    [Dependency] private readonly HandsSystem _handsSystem = default!;
    [Dependency] private readonly PolymorphSystem _polymorphSystem = default!;
    [Dependency] private readonly StaminaSystem _staminaSystem = default!;

    private readonly EntProtoId _mistFormAction = "ActionMistForm";
    private readonly EntProtoId _hypnoticAction = "ActionHypnoticGaze";
    private readonly ProtoId<PolymorphPrototype> _mistFormPolymorphId = "VampireMistForm";
    private readonly ProtoId<PolymorphPrototype> _mistFormForcedPolymorphId = "VampireMistFormForced";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VampireComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<VampireComponent, VampireMistFormActionEvent>(OnMistFormAction);
        SubscribeLocalEvent<VampireComponent, VampireHypnoticActionEvent>(OnHypnoticGazeAction);

        SubscribeLocalEvent<VampireComponent, MobStateChangedEvent>(OnMobStateChanged);

        SubscribeLocalEvent<VampireComponent, BloodDrainedEvent>(OnBloodDrained);

        SubscribeLocalEvent<VampireComponent, DamageModifyEvent>(OnDamageModified);

        SubscribeLocalEvent<VampireCoffinComponent, StartCollideEvent>(OnCoffinCollide);
    }

    private void OnMapInit(Entity<VampireComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, _mistFormAction);
        if (!ent.Comp.IsLesserVampire)
        {
            // Only progenitors get the hypnotic gaze ability
            // TODO: What if someone diabolerizes their progenitor?
            _actions.AddAction(ent, _hypnoticAction);
        }
    }

    private void OnMistFormAction(Entity<VampireComponent> ent, ref VampireMistFormActionEvent args)
    {
        if (ActivateMistForm(ent, false))
            args.Handled = true;
    }

    private void OnMobStateChanged(Entity<VampireComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Critical)
        {
            // TODO: Probably need more checks here for validity
            ActivateMistForm(ent, true);
        }
    }

    private bool ActivateMistForm(Entity<VampireComponent> ent, bool forced = false)
    {
        _handsSystem.TryDrop(ent.Owner);

        var proto = forced ? _mistFormForcedPolymorphId : _mistFormPolymorphId;
        var mistEnt = _polymorphSystem.PolymorphEntity(ent, proto);
        if (!mistEnt.HasValue)
            return false;

        // Propagate bonus resistances and other important info
        var mistVampire = EnsureComp<VampireComponent>(mistEnt.Value);
        mistVampire.BonusResistances = ent.Comp.BonusResistances;
        mistVampire.IsLesserVampire = ent.Comp.IsLesserVampire;
        mistVampire.IsForcedMistForm = forced;

        return true;
    }

    private void OnHypnoticGazeAction(Entity<VampireComponent> ent, ref VampireHypnoticActionEvent args)
    {
        var victim = args.Target;

        if (victim == ent.Owner)
            return; // Can't use your gaze against yourself

        if (TryComp<BlindableComponent>(victim, out var blindable) && blindable.IsBlind)
            return; // Can't use your gaze against a blind entity

        var bonusStaminaDamage = ent.Comp.BonusHypnoticDamageScale * ent.Comp.UniqueVictims.Count;
        _staminaSystem.TakeStaminaDamage(
            victim,
            ent.Comp.BaseHypnoticDamage + bonusStaminaDamage,
            null,
            ent
        );

        args.Handled = true;
    }

    private void OnBloodDrained(Entity<VampireComponent> ent, ref BloodDrainedEvent args)
    {
        var victim = args.Victim;

        // Ensure polymorphed victims only count once
        if (TryComp<PolymorphedEntityComponent>(victim, out var polymorphed))
            victim = polymorphed.Parent;

        if (ent.Comp.UniqueVictims.Add(victim))
            OnNewUniqueVictim(ent);

        ent.Comp.LastDrainedTime = GameTiming.CurTime;

        // TODO: Heal the drainer as they metabolize the Blood?? Done via other events?
        // TODO: Attempt to steal any Psionic abilities

        Dirty(ent);
    }

    private void OnNewUniqueVictim(Entity<VampireComponent> ent)
    {
        var bonusResists = ent.Comp.BonusResistancesPerUnique * ent.Comp.UniqueVictims.Count;
        if (bonusResists > ent.Comp.MaximumBonusResists)
        {
            bonusResists = ent.Comp.MaximumBonusResists;
        }

        ent.Comp.BonusResistances.Coefficients["Blunt"] = 1 - bonusResists;
        ent.Comp.BonusResistances.Coefficients["Slash"] = 1 - bonusResists;
        ent.Comp.BonusResistances.Coefficients["Pierce"] = 1 - bonusResists;
    }

    private void OnDamageModified(Entity<VampireComponent> ent, ref DamageModifyEvent args)
    {
        args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage, ent.Comp.BonusResistances);
    }

    private void OnCoffinCollide(Entity<VampireCoffinComponent> ent, ref StartCollideEvent args)
    {
        if (!TryComp<VampireComponent>(args.OtherEntity, out var vampire) || !vampire.IsForcedMistForm)
            return; // Not a vampire, or isn't currently forced into mist form

        // This vampire is in a critical state and has been forced into a mist, shut the coffin and regenerate the vampire.
        var original = _polymorphSystem.Revert(args.OtherEntity);
        if (!original.HasValue)
            return; // Failed to polymorph back

        _entityStorageSystem.CloseStorage(ent);
        _entityStorageSystem.Insert(original.Value, ent);

        // TODO: Heal the vampire slightly to get them OUT of crit.
        // TODO: Or put a slight heal for any vampire inside a coffin.
    }
}
