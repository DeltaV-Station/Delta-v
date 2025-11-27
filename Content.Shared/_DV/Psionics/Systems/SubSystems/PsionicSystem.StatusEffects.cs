using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._DV.Psionics.Systems;

public sealed partial class PsionicSystem
{
    public void InitializeStatusEffects()
    {
        SubscribeLocalEvent<PsionicsDisabledComponent, StatusEffectRelayedEvent<PsionicPowerUseAttemptEvent>>(OnPowerUseAttempt);
        SubscribeLocalEvent<ShieldedFromPsionicsComponent, StatusEffectRelayedEvent<TargetedByPsionicPowerEvent>>(OnTargetedByPsionicPower);
    }

    private void OnPowerUseAttempt(Entity<PsionicsDisabledComponent> psionic, ref StatusEffectRelayedEvent<PsionicPowerUseAttemptEvent> args)
    {
        args.Args.CanUsePower = false;
    }

    private void OnTargetedByPsionicPower(Entity<ShieldedFromPsionicsComponent> psionic, ref StatusEffectRelayedEvent<TargetedByPsionicPowerEvent> args)
    {
        args.Args.IsShielded = true;
    }
}
