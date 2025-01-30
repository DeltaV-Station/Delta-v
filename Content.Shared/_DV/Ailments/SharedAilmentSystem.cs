using Content.Shared._DV.Ailments;
using Content.Shared.Damage;
using Content.Shared.EntityEffects;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Shared._DV.Ailments;

public abstract class SharedAilmentSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AilmentComponent, DamageChangedEvent>(OnDamageChanged);
    }

    private void OnDamageChanged(Entity<AilmentComponent> ent, ref DamageChangedEvent args)
    {
        EvaluateAilments(ent);
    }

    private void EvaluateAilments(Entity<AilmentComponent> ent)
    {
        var args = new EntityEffectBaseArgs(ent.Owner, EntityManager);
        foreach (var packId in ent.Comp.Packs)
        {
            var pack = _protoMan.Index(packId);
            foreach (var transitionId in pack.Transitions)
            {
                EvaluatePotentialTransition(ent, packId, transitionId, args);
            }
        }
    }

    public List<string> HealthAnalyzerMessages(EntityUid uid)
    {
        var items = new List<string>();
        if (!TryComp<AilmentComponent>(uid, out var ailments))
            return items;

        foreach (var (_, ailment) in ailments.ActiveAilments)
        {
            if (ailment is null)
                continue;

            if (_protoMan.Index(ailment.Value).LocalizedHealthAnalyzerDescription is {} desc)
                items.Add(desc);
        }
        return items;
    }

    public bool ValidateTransition(Entity<AilmentComponent> ent, ProtoId<AilmentPackPrototype> pack, ProtoId<AilmentTransitionPrototype> transitionId, EntityEffectBaseArgs args)
    {
        if (!_protoMan.TryIndex(transitionId, out var transition))
            return false;

        var activeAilment = ent.Comp.ActiveAilments.GetValueOrDefault(pack);
        var startMatches = transition.Start == activeAilment;
        var endMatches = transition.End != activeAilment;
        var conditionsMatch = transition.Conditions.All(condition => condition.Condition(args));

        return startMatches && endMatches && conditionsMatch;
    }

    public bool TryTransition(Entity<AilmentComponent> ent, ProtoId<AilmentPackPrototype> pack, ProtoId<AilmentTransitionPrototype> transitionId, EntityEffectBaseArgs args)
    {
        if (!_protoMan.TryIndex(transitionId, out var transition))
            return false;

        if (!ValidateTransition(ent, pack, transitionId, args))
            return false;

        if (transition.Start is ProtoId<AilmentPrototype> startAilment)
        {
            RemoveAilment(ent, startAilment, args);
        }
        if (transition.End is ProtoId<AilmentPrototype> endAilment)
        {
            AddAilment(ent, endAilment, args);
        }

        ent.Comp.ActiveAilments[pack] = transition.End;

        if (transition.Effects is not null) foreach (var effect in transition.Effects)
        {
            effect.Effect(args);
        }

        Dirty(ent);
        return true;
    }

    private void EvaluatePotentialTransition(Entity<AilmentComponent> ent, ProtoId<AilmentPackPrototype> pack, ProtoId<AilmentTransitionPrototype> transitionId, EntityEffectBaseArgs args)
    {
        if (!_protoMan.TryIndex(transitionId, out var transition))
            return;

        if (transition.Triggers is null)
            return;

        var triggersMatch = transition.Triggers.All(condition => condition.Condition(args));

        if (triggersMatch && _random.NextFloat() <= transition.TriggerChance)
        {
            TryTransition(ent, pack, transitionId, args);
        }
    }

    private void RemoveAilment(Entity<AilmentComponent> ent, ProtoId<AilmentPrototype> ailmentId, EntityEffectBaseArgs args)
    {
        var ailment = _protoMan.Index(ailmentId);
        if (ailment.Unmount is not null) foreach (var effect in ailment.Unmount)
        {
            effect.Effect(args);
        }
    }

    private void AddAilment(Entity<AilmentComponent> ent, ProtoId<AilmentPrototype> ailmentId, EntityEffectBaseArgs args)
    {
        var ailment = _protoMan.Index(ailmentId);
        if (ailment.Mount is not null) foreach (var effect in ailment.Mount)
        {
            effect.Effect(args);
        }
    }
}
