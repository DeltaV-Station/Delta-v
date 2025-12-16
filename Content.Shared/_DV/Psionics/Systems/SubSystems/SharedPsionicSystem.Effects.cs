using Content.Shared._DV.Chemistry.Effects;
using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.EntityEffects;
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

        SubscribeLocalEvent<ExecuteEntityEffectEvent<ChemRemovePsionics>>(OnChemRemovePsionics);
        SubscribeLocalEvent<ExecuteEntityEffectEvent<ChemRollPsionic>>(OnChemRollPsionic);
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

    private void OnChemRemovePsionics(ref ExecuteEntityEffectEvent<ChemRemovePsionics> args)
    {
        if (args.Args is EntityEffectReagentArgs reagentArgs)
        {
            if (reagentArgs.Scale != 1f)
                return;
        }

        var ev = new PsionicMindBrokenEvent(stun: true);
        RaiseLocalEvent(args.Args.TargetEntity, ref ev);
    }

    private void OnChemRollPsionic(ref ExecuteEntityEffectEvent<ChemRollPsionic> args)
    {
        if (args.Args is EntityEffectReagentArgs reagentArgs)
        {
            if (reagentArgs.Scale != 1f)
                return;
        }

        if (!TryComp<PotentialPsionicComponent>(args.Args.TargetEntity, out var potComp))
            return;

        TryRollPsionic((args.Args.TargetEntity, potComp), args.Effect.BonusMultiplier);
    }
}
