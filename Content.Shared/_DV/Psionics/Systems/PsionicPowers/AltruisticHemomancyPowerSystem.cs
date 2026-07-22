using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared._DV.Psionics.Events.PowerDoAfterEvents;
using Content.Shared.Atmos.Rotting;
using Content.Shared.Body.Systems;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;

namespace Content.Shared._DV.Psionics.Systems.PsionicPowers;

public sealed class AltruisticHemomancyPowerSystem : BasePsionicPowerSystem<AltruisticHemomancyPowerComponent, AltruisticHemomancyPowerActionEvent>
{
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedRottingSystem _rotting = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AltruisticHemomancyPowerComponent, AltruisticHemomancyDoAfterEvent>(OnDoAfter);
    }

    protected override void OnPowerInit(Entity<AltruisticHemomancyPowerComponent> power, ref MapInitEvent args)
    {
        base.OnPowerInit(power, ref args);

        Popup.PopupEntity(Loc.GetString("psionic-power-altruistic-hemomancy-init"), power.Owner, power.Owner, PopupType.LargeCaution);
    }

    protected override void OnPowerUsed(Entity<AltruisticHemomancyPowerComponent> psionic, ref AltruisticHemomancyPowerActionEvent args)
    {
        if (!TryComp<DamageableComponent>(args.Target, out var damageable)
            || !Psionic.CanBeTargeted(args.Target, hasAggressor: args.Performer))
            return;

        var damage = _damageable.GetPositiveDamage((args.Target, damageable));

        if (!damage.AnyPositive())
        {
            Popup.PopupClient(Loc.GetString("psionic-power-altruistic-hemomancy-healthy"), args.Performer, args.Performer);
            return;
        }

        var doAfterDuration = psionic.Comp.DoAfterDuration;
        if (_mobState.IsCritical(args.Target))
            doAfterDuration *= psionic.Comp.CriticalDoAfterDurationModifier;
        else if (_mobState.IsDead(args.Target))
            doAfterDuration *= psionic.Comp.RotDoAfterDurationModifier;

        var doAfterArgs = new DoAfterArgs(EntityManager,
            args.Performer,
            doAfterDuration,
            new AltruisticHemomancyDoAfterEvent(),
            psionic,
            args.Target)
        {
            BreakOnMove = true,
            BreakOnWeightlessMove = false,
        };

        if (!DoAfter.TryStartDoAfter(doAfterArgs, out var id))
            return;

        psionic.Comp.SaveDoAfterId(id.Value);
        Dirty(psionic);

        var messageSelf = Loc.GetString("psionic-power-altruistic-hemomancy-start-self", ("target", Identity.Entity(args.Target, EntityManager)));
        var messageOthers = Loc.GetString("psionic-power-altruistic-hemomancy-start-others",
            ("user", Identity.Entity(args.Performer, EntityManager)),
            ("target", Identity.Entity(args.Target, EntityManager)));

        Popup.PopupPredicted(messageSelf, messageOthers, args.Performer, args.Performer, PopupType.MediumCaution);
        AfterPowerUsed(psionic, args.Performer);
    }

    private void OnDoAfter(Entity<AltruisticHemomancyPowerComponent> psionic, ref AltruisticHemomancyDoAfterEvent args)
    {
        if (args.Target is not {} target
            || args.Cancelled
            || args.Handled
            || !Psionic.CanBeTargeted(target, hasAggressor: args.User)
            || !TryComp<DamageableComponent>(target, out var damageable)
            || !_damageable.GetPositiveDamage((target, damageable)).AnyPositive())
            return;

        args.Handled = true;

        // TODO: Fix this when Upstream fixes the prediction issue with this.
        // I cannot build in a check for sufficient bloodlevel as of now. GetBloodLevel() causes missprediction issues.
        //if (_bloodstream.GetBloodLevel(args.User) < psionic.Comp.MinBloodPercentage)
        // {
        //     Popup.PopupClient(Loc.GetString("psionic-power-altruistic-hemomancy-insufficient-blood"), args.User, args.User, PopupType.SmallCaution);
        //     return;
        // }

        if (_mobState.IsDead(target))
            HealRot(psionic, args.User, target);
        else
            HealDamage(psionic, args.User, target, damageable);

        psionic.Comp.TickCounter++;

        if (psionic.Comp.TickCounter < psionic.Comp.MaxHealingTicks && _damageable.GetPositiveDamage((target, damageable)).AnyPositive())
        {
            args.Args.Delay = psionic.Comp.DoAfterDuration;

            if (_mobState.IsCritical(target))
                args.Args.Delay *= psionic.Comp.CriticalDoAfterDurationModifier;
            else if (_mobState.IsDead(target))
                args.Args.Delay *= psionic.Comp.RotDoAfterDurationModifier;

            args.Repeat = true;
            Dirty(psionic);
            return;
        }

        psionic.Comp.TickCounter = 0;
        psionic.Comp.RemoveSavedDoAfterId();
        Dirty(psionic);

        var messageSelf = Loc.GetString("psionic-power-altruistic-hemomancy-end-self");
        var messageOthers = Loc.GetString("psionic-power-altruistic-hemomancy-end-others",
            ("user", Identity.Entity(args.User, EntityManager)));

        Popup.PopupPredicted(messageSelf, messageOthers, args.User, args.User, PopupType.SmallCaution);
    }

    private void HealDamage(Entity<AltruisticHemomancyPowerComponent> psionic, EntityUid user, EntityUid target, DamageableComponent damageable)
    {
        var bloodPercentageCost = psionic.Comp.BloodCost;
        if (_mobState.IsCritical(target))
            bloodPercentageCost *= psionic.Comp.CriticalHealingCostModifier;

        if (!_bloodstream.TryModifyBloodLevel(user, bloodPercentageCost))
            return;
        // Damage the user for every heal.
        _damageable.TryChangeDamage(user, psionic.Comp.DamageOnHealing, interruptsDoAfters: false, origin: psionic);
        _bloodstream.TryModifyBleedAmount(target, psionic.Comp.ReducedBleeding);

        foreach (var groupHeal in psionic.Comp.Heal)
        {
            _damageable.HealEvenly((target, damageable), groupHeal.Value, groupHeal.Key, user);
        }
    }

    private void HealRot(Entity<AltruisticHemomancyPowerComponent> psionic, EntityUid user, EntityUid target)
    {
        if (!_bloodstream.TryModifyBloodLevel(user, psionic.Comp.BloodCost * psionic.Comp.RotHealingCostModifier))
            return;

        Popup.PopupClient(Loc.GetString("psionic-power-altruistic-hemomancy-rot-damage"), user, user, PopupType.SmallCaution);

        _rotting.ReduceAccumulator(target, psionic.Comp.ReduceRot);
        _damageable.TryChangeDamage(user, psionic.Comp.DamageOnHealingRot, true, false, user);
    }
}
