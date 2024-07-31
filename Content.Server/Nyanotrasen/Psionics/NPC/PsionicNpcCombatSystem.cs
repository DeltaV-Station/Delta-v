using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions;
using Content.Server.NPC.Events;
using Content.Server.NPC.Components;
using Content.Server.Abilities.Psionics;
using Robust.Shared.Timing;

namespace Content.Server.Psionics.NPC;

public sealed class PsionicNpcCombatSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NoosphericZapPowerComponent, NPCSteeringEvent>(ZapCombat);
    }

    // TODO: would be useful if this can be reused for other powers like pyrokinesis wyci
    private void ZapCombat(Entity<NoosphericZapPowerComponent> ent, ref NPCSteeringEvent args)
    {
        var (uid, comp) = ent;
        if (comp.NoosphericZapActionEntity is not {} action)
            return;

        // TODO: when action refactor is merged and cherry picked update this to get ActionComponent
        var target = Comp<EntityTargetActionComponent>(action);
        if (target.Cooldown is {} cooldown && cooldown.End > _timing.CurTime)
            return;

        if (!TryComp<NPCRangedCombatComponent>(uid, out var combat))
            return;

        if (!_actions.ValidateEntityTarget(uid, combat.Target, (action, target)))
            return;

        if (target.Event is not {} ev)
            return;

        ev.Target = combat.Target;
        _actions.PerformAction(uid, null, action, target, ev, _timing.CurTime, predicted: false);
    }
}
