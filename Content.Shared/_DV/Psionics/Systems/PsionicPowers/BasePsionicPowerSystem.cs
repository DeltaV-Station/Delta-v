using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.Actions;
using Content.Shared.Administration.Logs;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Psionics.Glimmer;
using Robust.Shared.Random;
using Robust.Shared.Timing;

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
    [Dependency] private readonly IGameTiming _timing = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<T, MapInitEvent>(OnPowerInit);

        SubscribeLocalEvent<T, T1>(OnPowerActionUsed);
        SubscribeLocalEvent<T, PsionicMindBrokenEvent>(OnMindBroken);
        SubscribeLocalEvent<T, GetItemActionsEvent>(OnGrantingClothingEquipped);
    }

    /// <summary>
    /// This is called on every power upon initialization, so the action gets put into the action container.
    /// The only exemption is powers that don't have just one action.
    /// </summary>
    /// <param name="power">The psionic power whose action is put into the container.</param>
    /// <param name="args">The args from the event.</param>
    protected virtual void OnPowerInit(Entity<T> power, ref MapInitEvent args)
    {
        ActionSystem.AddAction(power, ref power.Comp.ActionEntity, power.Comp.ActionProtoId );

        var psionicComp = EnsureComp<PsionicComponent>(power);
        psionicComp.PsionicPowersActionEntities.Add(power.Comp.ActionEntity);
        Dirty(power);
    }

    /// <summary>
    /// This is called whenever an entity pushes the psionic power action button.
    /// </summary>
    /// <param name="psionic">The psionic who attempts to use a psionic power.</param>
    /// <param name="args">The action event for said power.</param>
    private void OnPowerActionUsed(Entity<T> psionic,  ref T1 args)
    {
        if (_timing.ApplyingState)
            return;

        var ev = new PsionicPowerUseAttemptEvent();
        RaiseLocalEvent(args.Performer, ref ev);

        if (ev.CanUsePower)
        {
            OnPowerUsed(psionic, ref args);
            return;
        }

        PopupSystem.PopupClient(Loc.GetString("psionic-cannot-use-psionics"), args.Performer);
    }

    protected abstract void OnPowerUsed(Entity<T> psionic, ref T1 args);

    protected void OnGrantingClothingEquipped(Entity<T> psionicClothing, ref GetItemActionsEvent args)
    {
        if (args.SlotFlags is null or SlotFlags.POCKET
            || !HasComp<PotentialPsionicComponent>(args.User)) // IPCs and non-player organics shouldn't be able to use abilities.
            return;

        args.AddAction(psionicClothing.Comp.ActionEntity);
        Dirty(psionicClothing);
    }

    public void OnMindBroken(Entity<T> psionic, ref PsionicMindBrokenEvent args)
    {
        ActionSystem.RemoveAction(psionic.Comp.ActionEntity);
        RemComp<T>(psionic);
    }

    public void LogPowerUsed(EntityUid psionicSource, EntityUid performer, string power, int minGlimmer = 8, int maxGlimmer = 12)
    {
        power = Loc.GetString(power);
        AdminLogger.Add(Database.LogType.Psionics, Database.LogImpact.Medium, $"{ToPrettyString(psionicSource):player} used {power}");

        var ev = new PsionicPowerUsedEvent(performer, psionicSource, power);
        RaiseLocalEvent(psionicSource, ev);

        GlimmerSystem.Glimmer += Random.Next(minGlimmer, maxGlimmer);
    }
}
