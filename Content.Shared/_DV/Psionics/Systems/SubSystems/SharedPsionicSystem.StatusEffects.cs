using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._DV.Psionics.Systems;

public abstract partial class SharedPsionicSystem
{
    public void InitializeStatusEffects()
    {
        SubscribeLocalEvent<PsionicsDisabledComponent, StatusEffectRelayedEvent<PsionicPowerUseAttemptEvent>>(OnPowerUseAttempt);
        SubscribeLocalEvent<ShieldedFromPsionicsComponent, StatusEffectRelayedEvent<TargetedByPsionicPowerEvent>>(OnTargetedByPsionicPower);
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
}
