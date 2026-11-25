using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Systems;

public abstract partial class SharedPsionicSystem
{
    public static readonly EntProtoId PsionicsDisabledProtoId = "StatusEffectPsionicsDisabled";
    public void InitializeStatusEffects()
    {
        SubscribeLocalEvent<PsionicsDisabledComponent, StatusEffectRelayedEvent<PsionicPowerUseAttemptEvent>>(OnPowerUseAttempt);
        SubscribeLocalEvent<ShieldedFromPsionicsComponent, StatusEffectRelayedEvent<TargetedByPsionicPowerEvent>>(OnTargetedByPsionicPower);

        SubscribeLocalEvent<PsionicsDisabledComponent, StatusEffectAppliedEvent>(OnStatusEffectApplied);
        SubscribeLocalEvent<PsionicsDisabledComponent, StatusEffectRemovedEvent>(OnStatusEffectRemoved);
        SubscribeLocalEvent<ShieldedFromPsionicsComponent, StatusEffectAppliedEvent>(OnStatusEffectApplied);
        SubscribeLocalEvent<ShieldedFromPsionicsComponent, StatusEffectRemovedEvent>(OnStatusEffectRemoved);
    }

    private void OnPowerUseAttempt(Entity<PsionicsDisabledComponent> psionic, ref StatusEffectRelayedEvent<PsionicPowerUseAttemptEvent> args)
    {
        var ev = args.Args;
        ev.CanUsePower = false;
        args.Args = ev;
    }

    private void OnTargetedByPsionicPower(Entity<ShieldedFromPsionicsComponent> psionic, ref StatusEffectRelayedEvent<TargetedByPsionicPowerEvent> args)
    {
        var ev = args.Args;
        ev.IsShielded = true;
        args.Args = ev;
    }

    private void OnStatusEffectApplied(Entity<PsionicsDisabledComponent> statusEffect, ref StatusEffectAppliedEvent args)
    {
        var ev = new PsionicSuppressedEvent(args.Target);
        RaiseLocalEvent(args.Target, ref ev);
    }

    private void OnStatusEffectRemoved(Entity<PsionicsDisabledComponent> statusEffect, ref StatusEffectRemovedEvent args)
    {
        if (!CanUsePsionicAbility(args.Target))
            return;

        var ev = new PsionicStoppedSuppressedEvent(args.Target);
        RaiseLocalEvent(args.Target, ref ev);
    }

    private void OnStatusEffectApplied(Entity<ShieldedFromPsionicsComponent> statusEffect, ref StatusEffectAppliedEvent args)
    {
        var ev = new PsionicShieldedEvent(args.Target);
        RaiseLocalEvent(args.Target, ref ev);
    }

    private void OnStatusEffectRemoved(Entity<ShieldedFromPsionicsComponent> statusEffect, ref StatusEffectRemovedEvent args)
    {
        if (!CanBeTargeted(args.Target, showPopup: false))
            return;

        var ev = new PsionicStoppedShieldedEvent(args.Target);
        RaiseLocalEvent(args.Target, ref ev);
    }
}
