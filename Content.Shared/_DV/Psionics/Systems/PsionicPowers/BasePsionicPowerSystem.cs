using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.Actions;
using Content.Shared.Administration.Logs;
using Content.Shared.Popups;
using Content.Shared.Psionics.Glimmer;
using Robust.Shared.Random;

namespace Content.Shared._DV.Psionics.Systems.PsionicPowers;

/// <summary>
/// This is a base psionic power system that handles being mindbroken and checks for being able to use psionic powers automagically!
/// </summary>
public abstract class BasePsionicPowerSystem<T, T1> : EntitySystem where T : BasePsionicPowerComponent where T1 : BaseActionEvent
{
    [Dependency] protected readonly ISharedAdminLogManager AdminLogger = default!;
    [Dependency] protected readonly IRobustRandom Random = default!;
    [Dependency] protected readonly SharedActionsSystem ActionSystem = default!;
    [Dependency] protected readonly SharedPopupSystem PopupSystem = default!;
    [Dependency] protected readonly GlimmerSystem GlimmerSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<T, MapInitEvent>(OnPowerInit);

        SubscribeLocalEvent<T, T1>(OnPowerActionUsed);
        SubscribeLocalEvent<T, PsionicMindBrokenEvent>(OnMindBroken);
    }

    /// <summary>
    /// This is called on every power upon initialization, so the action gets put into the action container.
    /// The only exemption is powers that don't have just one action.
    /// </summary>
    /// <param name="power">The psionic power whose action is put into the container.</param>
    /// <param name="args">The args from the event.</param>
    private void OnPowerInit(Entity<T> power, ref MapInitEvent args)
    {
        ActionSystem.AddAction(power, ref power.Comp.ActionEntity, power.Comp.ActionProtoId );

        var psionicComp = EnsureComp<PsionicComponent>(power);
        psionicComp.PsionicPowersActionEntities.Add(power.Comp.ActionEntity);
    }

    /// <summary>
    /// This is called whenever an entity pushes the psionic power action button.
    /// </summary>
    /// <param name="psionic">The psionic who attempts to use a psionic power.</param>
    /// <param name="args">The action event for said power.</param>
    private void OnPowerActionUsed(Entity<T> psionic,  ref T1 args)
    {
        var ev = new PsionicPowerUseAttemptEvent();
        RaiseLocalEvent(psionic.Owner, ref ev);

        if (ev.CanUsePower)
        {
            OnPowerUsed(psionic, ref args);
            return;
        }

        PopupSystem.PopupClient(Loc.GetString("psionic-cannot-use-psionics"), psionic);
    }

    protected abstract void OnPowerUsed(Entity<T> psionic, ref T1 args);

    public void OnMindBroken(Entity<T> psionic, ref PsionicMindBrokenEvent args)
    {
        ActionSystem.RemoveAction(psionic.Comp.ActionEntity);
        RemComp(psionic.Owner, psionic.Comp);
    }

    public void LogPowerUsed(EntityUid psionic, string power, int minGlimmer = 8, int maxGlimmer = 12)
    {
        AdminLogger.Add(Database.LogType.Psionics, Database.LogImpact.Medium, $"{ToPrettyString(psionic):player} used {power}");

        var ev = new PsionicPowerUsedEvent(psionic, power);
        RaiseLocalEvent(psionic, ev);

        GlimmerSystem.Glimmer += Random.Next(minGlimmer, maxGlimmer);
    }
}
