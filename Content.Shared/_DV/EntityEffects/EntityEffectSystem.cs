using Content.Shared._DV.Chemistry.Effects;
using Content.Shared._DV.Psionics.Events;
using Content.Shared._DV.Psionics.Systems;
using Content.Shared.EntityEffects;

namespace Content.Shared._DV.EntityEffects;

public sealed class EntityEffectSystem : EntitySystem
{
    [Dependency] private readonly PsionicSystem  _psionicSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecuteEntityEffectEvent<ChemRemovePsionics>>(OnChemRemovePsionics);
        SubscribeLocalEvent<ExecuteEntityEffectEvent<ChemRollPsionic>>(OnChemRollPsionic);
    }

    private void OnChemRemovePsionics(ref ExecuteEntityEffectEvent<ChemRemovePsionics> args)
    {
        if (args.Args is EntityEffectReagentArgs reagentArgs)
        {
            if (reagentArgs.Scale != 1f)
                return;
        }

        var ev = new PsionicMindBrokenEvent();
        RaiseLocalEvent(args.Args.TargetEntity, ref ev);
    }

    private void OnChemRollPsionic(ref ExecuteEntityEffectEvent<ChemRollPsionic> args)
    {
        // var psySys = args.Args.EntityManager.EntitySysManager.GetEntitySystem<PsionicsSystem>();
        // psySys.RerollPsionics(args.Args.TargetEntity, bonusMuliplier: args.Effect.BonusMuliplier);
    }
}
