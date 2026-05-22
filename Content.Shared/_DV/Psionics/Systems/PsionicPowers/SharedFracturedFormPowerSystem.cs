using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared._DV.Psionics.Events.PowerDoAfterEvents;
using Content.Shared.Bed.Sleep;
using Content.Shared.Chat;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.SSDIndicator;
using Robust.Shared.Player;

namespace Content.Shared._DV.Psionics.Systems.PsionicPowers;

public abstract class SharedFracturedFormPowerSystem : BasePsionicPowerSystem<FracturedFormPowerComponent, FracturedFormPowerActionEvent>
{
    [Dependency] protected readonly SharedChatSystem Chat = default!;
    [Dependency] protected readonly MobStateSystem MobState = default!;
    [Dependency] protected readonly SleepingSystem Sleeping = default!;

    protected EntityQuery<FracturedFormPowerComponent> FracturedQuery;
    private EntityQuery<ForcedSleepingStatusEffectComponent> _forcedSleepQuery;
    protected EntityQuery<MindContainerComponent> MindContainerQuery;
    private EntityQuery<MobStateComponent> _mobStateQuery;
    protected EntityQuery<SSDIndicatorComponent> SsdQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FracturedFormPowerComponent, FracturedFormDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<FracturedFormBodyComponent, PlayerDetachedEvent>(OnPlayerDetached, after: [typeof(SSDIndicatorSystem)]);
        SubscribeLocalEvent<FracturedFormBodyComponent, ExaminedEvent>(OnExamine);

        FracturedQuery = GetEntityQuery<FracturedFormPowerComponent>();
        _forcedSleepQuery = GetEntityQuery<ForcedSleepingStatusEffectComponent>();
        MindContainerQuery = GetEntityQuery<MindContainerComponent>();
        _mobStateQuery = GetEntityQuery<MobStateComponent>();
        SsdQuery = GetEntityQuery<SSDIndicatorComponent>();
    }

    protected override void OnPowerUsed(Entity<FracturedFormPowerComponent> psionic, ref FracturedFormPowerActionEvent args)
    {
        if (!CanSwap(psionic))
        {
            Popup.PopupClient(Loc.GetString("psionic-power-fractured-form-nobodies"), psionic, args.Performer, PopupType.Large);
            return;
        }

        Chat.TryEmoteWithChat(psionic.Owner, "Yawn");
        Popup.PopupClient(Loc.GetString("psionic-power-fractured-form-sleepy"), psionic, args.Performer, PopupType.LargeCaution);

        var doAfterArgs = new DoAfterArgs(EntityManager, args.Performer, psionic.Comp.ManualSwapTime, new FracturedFormDoAfterEvent(), psionic);

        if (!DoAfter.TryStartDoAfter(doAfterArgs, out var doAfterId))
            return;

        psionic.Comp.SaveDoAfterId(doAfterId.Value);
        Dirty(psionic);
        AfterPowerUsed(psionic, args.Performer);
    }

    private void OnDoAfter(Entity<FracturedFormPowerComponent> psionic, ref FracturedFormDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;
        args.Handled = true;
        psionic.Comp.RemoveSavedDoAfterId();

        Dirty(psionic);
        Sleeping.TrySleeping(psionic.Owner);
    }

    private void OnPlayerDetached(Entity<FracturedFormBodyComponent> body, ref PlayerDetachedEvent args)
    {
        if (!SsdQuery.TryComp(body, out var comp)
            || FracturedQuery.HasComp(body))
            return;

        comp.IsSSD = false;
    }

    private void OnExamine(Entity<FracturedFormBodyComponent> psionic, ref ExaminedEvent args)
    {
        if (HasComp<FracturedFormPowerComponent>(psionic))
            return;

        if (TryComp<FracturedFormPowerComponent>(args.Examiner, out var fracturedHost) && fracturedHost.Bodies.Contains(psionic.Owner))
            args.PushMarkup($"[color=yellow]{Loc.GetString("psionic-power-fractured-form-examine-self", ("ent", psionic))}[/color]");
        else
            args.PushMarkup($"[color=yellow]{Loc.GetString("psionic-power-fractured-form-ssd", ("ent", psionic))}[/color]");
    }

    /// <summary>
    /// We're overriding the dispelled method to introduce some special behavior.
    /// </summary>
    protected override void OnDispelled(Entity<FracturedFormPowerComponent> psionic, ref DispelledEvent args)
    {
        if (args.Handled)
            return;
        args.Handled = true;

        // This resets the swap timer.
        var randomTime = Random.Next(psionic.Comp.NextSwapMinTime, psionic.Comp.NextSwapMaxTime);
        psionic.Comp.NextSwap = Timing.CurTime + randomTime;

        if (psionic.Comp.GetDoAfterId() is not { } doAfterId)
        {
            Popup.PopupClient(Loc.GetString("psionic-power-fractured-form-dispelled"), psionic, PopupType.MediumCaution);
            return;
        }

        DoAfter.Cancel(doAfterId);
        Popup.PopupClient(Loc.GetString("psionic-dispelled"), psionic, PopupType.MediumCaution);
        psionic.Comp.RemoveSavedDoAfterId();
    }

    protected bool IsValidBody(Entity<FracturedFormPowerComponent> psionic, EntityUid otherBody)
    {
        if (otherBody == psionic.Owner)
            return false;
        if (!psionic.Comp.Bodies.Contains(otherBody))
            return false;
        if (!MindContainerQuery.TryComp(otherBody, out var cmind))
            return false;
        if (cmind.HasMind)
            return false;
        if (_forcedSleepQuery.HasComp(otherBody))
            return false;
        if (!_mobStateQuery.TryComp(otherBody, out var mobState))
            return false;
        if (MobState.IsIncapacitated(otherBody, mobState))
            return false;

        return true;
    }

    public bool CanSwap(Entity<FracturedFormPowerComponent> psionic)
    {
        foreach (var body in psionic.Comp.Bodies)
        {
            if (IsValidBody(psionic, body))
                return true;
        }
        return false;
    }
}
