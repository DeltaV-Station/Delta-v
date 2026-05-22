using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared._DV.Psionics.Events.PowerDoAfterEvents;
using Content.Shared._DV.Psionics.Systems.PsionicPowers;
using Content.Shared.Bed.Sleep;
using Content.Shared.DoAfter;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Psionics.Systems.PsionicPowers;

public sealed class MassSleepPowerSystem : SharedMassSleepPowerSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly Shared.StatusEffectNew.StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly MovementModStatusSystem _movementMod = default!;

    public static readonly EntProtoId MassSleepSlowdown = "MassSleepSlowdownStatusEffect";
    private EntityQuery<PsionicPowerDetectorComponent> _psionicDetectorQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MassSleepPowerComponent, MassSleepDoAfterEvent>(OnMassSleepDoAfter);
        _psionicDetectorQuery = GetEntityQuery<PsionicPowerDetectorComponent>();
    }

    protected override void OnPowerUsed(Entity<MassSleepPowerComponent> psionic, ref MassSleepPowerActionEvent args)
    {
        var ev = new MassSleepDoAfterEvent();
        var doAfterArgs = new DoAfterArgs(EntityManager, args.Performer, psionic.Comp.UseDelay, ev, args.Performer)
        {
            BreakOnDamage = true,
        };

        if (!DoAfter.TryStartDoAfter(doAfterArgs, out var doAfterId))
            return;

        foreach (var target in _lookup.GetEntitiesInRange(args.Performer, psionic.Comp.WarningRadius))
        {
            // TODO: Metapsionic Users won't see this message, as it would otherwise overlap their usual power detected popup. Fix it.
            if (args.Performer != target && Psionic.CanBeTargeted(target) && !_psionicDetectorQuery.HasComp(target))
            {
                Popup.PopupEntity(Loc.GetString("psionic-power-mass-sleep-warning"),
                    target,
                    target,
                    PopupType.LargeCaution);
            }
        }

        _movementMod.TryUpdateMovementSpeedModDuration(args.Performer, MassSleepSlowdown, psionic.Comp.UseDelay, 0.5f);

        psionic.Comp.SaveDoAfterId(doAfterId.Value);

        Dirty(psionic);
        AfterPowerUsed(psionic, args.Performer);
    }

    private void OnMassSleepDoAfter(Entity<MassSleepPowerComponent> psionic, ref MassSleepDoAfterEvent args)
    {
        if (args.Handled)
            return;
        args.Handled = true;
        psionic.Comp.RemoveSavedDoAfterId();
        Dirty(psionic);

        if (args.Cancelled)
        {
            _statusEffects.TryRemoveStatusEffect(psionic, MassSleepSlowdown);
            return;
        }

        foreach (var target in _lookup.GetEntitiesInRange(args.User, psionic.Comp.Radius))
        {
            if (args.Used != target && Psionic.CanBeTargeted(target))
                _statusEffects.TryUpdateStatusEffectDuration(target, SleepingSystem.StatusEffectForcedSleeping, psionic.Comp.Duration);
        }
    }
}
