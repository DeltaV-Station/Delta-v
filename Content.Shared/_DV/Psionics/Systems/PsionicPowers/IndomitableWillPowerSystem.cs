using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared.Body.Systems;
using Content.Shared.Cuffs;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._DV.Psionics.Systems.PsionicPowers;

/// <summary>
/// This handles the usage of the psionic power Indomitable Will.
/// </summary>
public sealed class IndomitableWillPowerSystem : BasePsionicPowerSystem<IndomitableWillPowerComponent, IndomitableWillPowerActionEvent>
{
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly SharedCuffableSystem _cuffable = default!;
    [Dependency] private readonly DamageableSystem  _damageable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    public static readonly EntProtoId CriticalEffectProtoId = "StatusEffectCriticalWill";
    public static readonly EntProtoId NormalEffectProtoId = "StatusEffectIndomitableWill";

    protected override void OnPowerInit(Entity<IndomitableWillPowerComponent> power, ref MapInitEvent args)
    {
        base.OnPowerInit(power, ref args);

        var action = Action.GetAction(power.Comp.ActionEntity);

        DebugTools.Assert(action != null);

        power.Comp.NormalUseDelay = action.Value.Comp.UseDelay;
    }

    protected override void OnPowerUsed(Entity<IndomitableWillPowerComponent> psionic, ref IndomitableWillPowerActionEvent args)
    {
        if (_mobState.IsCritical(args.Performer))
        {
            _statusEffects.TryUpdateStatusEffectDuration(args.Performer, CriticalEffectProtoId, psionic.Comp.CritBuffDuration);
            var messageSelf = Loc.GetString("psionic-power-indomitable-will-crit-self");
            var messageOthers = Loc.GetString("psionic-power-indomitable-will-crit-others", ("user", Identity.Entity(args.Performer, EntityManager)));

            Popup.PopupPredicted(messageSelf, messageOthers, args.Performer, args.Performer, PopupType.LargeCaution);
            Action.SetUseDelay(psionic.Comp.ActionEntity, psionic.Comp.SpecialUsageCooldown);
            AfterPowerUsed(psionic, args.Performer);
            return;
        }
        if (TryComp<CuffableComponent>(args.Performer, out var cuffable) && _cuffable.IsCuffed((args.Performer, cuffable)))
        {
            if (!_cuffable.TryGetLastCuff((args.Performer, cuffable), out var cuff))
                return;

            _cuffable.Uncuff(args.Performer, args.Performer, cuff.Value, cuffable);
            PredictedQueueDel(cuff);

            var messageSelf = Loc.GetString("psionic-power-indomitable-will-uncuff-self");
            var messageOthers = Loc.GetString("psionic-power-indomitable-will-uncuff-others", ("user", Identity.Entity(args.Performer, EntityManager)));

            Popup.PopupPredicted(messageSelf, messageOthers, args.Performer, args.Performer, PopupType.LargeCaution);
            _damageable.TryChangeDamage(args.Performer, psionic.Comp.UncuffDamage);
            Action.SetUseDelay(psionic.Comp.ActionEntity, psionic.Comp.SpecialUsageCooldown);
            AfterPowerUsed(psionic, args.Performer);
            return;
        }

        if (!_statusEffects.TryUpdateStatusEffectDuration(args.Performer, NormalEffectProtoId, psionic.Comp.BuffDuration))
            return;

        _bloodstream.FlushChemicals(args.Performer, psionic.Comp.ReagentRemoved);

        var ev = new IndomitableWillSuccessEvent();
        RaiseLocalEvent(args.Performer, ref ev);

        var messageNormalSelf = Loc.GetString("psionic-power-indomitable-will-self");
        var messageNormalOthers = Loc.GetString("psionic-power-indomitable-will-others", ("user", Identity.Entity(args.Performer, EntityManager)));

        Popup.PopupPredicted(messageNormalSelf, messageNormalOthers, args.Performer, args.Performer, PopupType.Medium);
        Action.SetUseDelay(psionic.Comp.ActionEntity, psionic.Comp.NormalUseDelay);
        AfterPowerUsed(psionic, args.Performer);
    }
}
