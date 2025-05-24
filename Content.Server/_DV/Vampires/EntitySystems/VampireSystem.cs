using Content.Server.Actions;
using Content.Server.Damage.Systems;
using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Shared._DV.BloodDraining.Events;
using Content.Shared._DV.Vampires.Components;
using Content.Shared._DV.Vampires.EntitySystems;
using Content.Shared._DV.Vampires.Events;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Mobs;
using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;


namespace Content.Server._DV.Vampires.EntitySystems;

public sealed class VampireSystem : SharedVampireSystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
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
        var proto = forced ? _mistFormForcedPolymorphId : _mistFormPolymorphId;
        if (_polymorphSystem.PolymorphEntity(ent, proto) == null)
            return false;

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
        // TODO: Update bonuses
    }
}
