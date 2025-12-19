using Content.Shared._DV.Abilities;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions;
using Content.Shared.Coordinates;
using Robust.Server.Audio;

namespace Content.Server._DV.Abilities;

public sealed partial class PsychokineticScreamPowerSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly ShatterLightsAbilitySystem _shatterLights = default!;
    [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PsychokineticScreamPowerComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<PsychokineticScreamPowerComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<PsychokineticScreamPowerComponent, ShatterLightsActionEvent>(OnShatterLightsAction);
    }

    private void OnMapInit(Entity<PsychokineticScreamPowerComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, ref ent.Comp.PsychokineticScreamActionEntity, ent.Comp.ShatterLightsActionId);
        _actions.StartUseDelay(ent.Comp.PsychokineticScreamActionEntity);

        if (TryComp<PsionicComponent>(ent, out var psionic) && psionic.PsionicAbility == null)
        {
            psionic.PsionicAbility = ent.Comp.PsychokineticScreamActionEntity;
            psionic.ActivePowers.Add(ent.Comp);
        }
    }

    private void OnShutdown(Entity<PsychokineticScreamPowerComponent> entity, ref ComponentShutdown args)
    {
        _actions.RemoveAction(entity.Owner, entity.Comp.PsychokineticScreamActionEntity);
        if (TryComp<PsionicComponent>(entity, out var psionic))
        {
            psionic.ActivePowers.Remove(entity.Comp);
        }
    }

    private void OnShatterLightsAction(Entity<PsychokineticScreamPowerComponent> entity, ref ShatterLightsActionEvent args)
    {
        if (args.Handled)
            return;

        if (entity.Comp.AbilitySound != null)
            _audio.PlayPvs(entity.Comp.AbilitySound, entity);

        _shatterLights.ShatterLightsAround(entity.Owner, entity.Comp.Radius, entity.Comp.LineOfSight);

        SpawnAttachedTo(entity.Comp.Effect, entity.Owner.ToCoordinates());

        _psionics.LogPowerUsed(entity.Owner, "psychokinetic scream", 3, 6);

        args.Handled = true;
    }

}
