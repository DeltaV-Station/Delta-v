using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events.PowerDoAfterEvents;
using Content.Shared.Actions.Events;
using Content.Shared.DoAfter;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.StatusEffect;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Systems.PsionicPowers;

/// <summary>
/// This solely exist for client-side action handling prediction.
/// </summary>
public abstract class SharedPrecognitionPowerSystem : BasePsionicPowerSystem<PrecognitionPowerComponent, PrecognitionPowerActionEvent>
{
    [Dependency] protected readonly SharedAudioSystem Audio = default!;
    [Dependency] protected readonly StatusEffectsSystem StatusEffects = default!;
    [Dependency] protected readonly MovementModStatusSystem Movement = default!;

    public static readonly EntProtoId PrecognitionSlowdown = "PrecognitionSlowdownStatusEffect";

    protected override void OnPowerUsed(Entity<PrecognitionPowerComponent> psionic, ref PrecognitionPowerActionEvent args)
    {
        var ev = new PrecognitionDoAfterEvent();
        var doAfterArgs = new DoAfterArgs(EntityManager, args.Performer, psionic.Comp.UseDelay, ev, psionic)
        {
            BreakOnDamage = true,
        };

        if (!DoAfter.TryStartDoAfter(doAfterArgs, out var doAfterId))
            return;

        // A custom shader for seeing visions would be nice but this will do for now.
        // TODO: Port over the TemporaryBlindness effect to the new StatusEffectSystem.
        // When Upstream ports it over, replace this with it.
        StatusEffects.TryAddStatusEffect<TemporaryBlindnessComponent>(psionic, "TemporaryBlindness", psionic.Comp.UseDelay, true);
        Movement.TryUpdateMovementSpeedModDuration(args.Performer, PrecognitionSlowdown, psionic.Comp.UseDelay, 0.5f);

        psionic.Comp.SaveDoAfterId(doAfterId.Value);

        var player = Audio.PlayGlobal(psionic.Comp.VisionSound, Filter.Entities(args.Performer), true);
        if (player != null)
            psionic.Comp.SoundStream = player.Value.Entity;

        Dirty(psionic);
        AfterPowerUsed(psionic, args.Performer);
    }
}
