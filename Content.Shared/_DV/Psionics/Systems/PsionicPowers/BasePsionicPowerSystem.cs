using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.Actions;
using Content.Shared.Administration.Logs;
using Content.Shared.DoAfter;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Psionics.Glimmer;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._DV.Psionics.Systems.PsionicPowers;

/// <summary>
/// This is a base psionic power system that handles being mindbroken and checks for being able to use psionic powers automagically!
/// You WILL NEED to parent of this.
/// </summary>
public abstract class BasePsionicPowerSystem<T, T1> : EntitySystem where T : BasePsionicPowerComponent where T1 : BaseActionEvent
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] protected readonly IRobustRandom Random = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] protected readonly SharedActionsSystem Action = default!;
    [Dependency] protected readonly SharedDoAfterSystem DoAfter = default!;
    [Dependency] private readonly GlimmerSystem _glimmer = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] protected readonly SharedPsionicSystem Psionic = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<T, MapInitEvent>(OnPowerInit);

        SubscribeLocalEvent<T, T1>(OnPowerActionUsed);
        SubscribeLocalEvent<T, DispelledEvent>(OnDispelled);
        SubscribeLocalEvent<T, PsionicMindBrokenEvent>(OnMindBroken);
        SubscribeLocalEvent<T, GetItemActionsEvent>(OnGrantingClothingEquipped);
        SubscribeLocalEvent<T, PsionicSuppressedEvent>(OnPsionicallySuppressed);
        SubscribeLocalEvent<T, PsionicStoppedSuppressedEvent>(OnStoppedPsionicallySuppressed);
    }

    /// <summary>
    /// This is called on every power upon initialization, so the action gets put into the action container.
    /// The only exemption is powers that don't have just one action or require special actions at initialize.
    /// </summary>
    /// <param name="power">The psionic power whose action is put into the container.</param>
    /// <param name="args">The args from the event.</param>
    protected virtual void OnPowerInit(Entity<T> power, ref MapInitEvent args)
    {
        Action.AddAction(power, ref power.Comp.ActionEntity, power.Comp.ActionProtoId);

        var psionicComp = EnsureComp<PsionicComponent>(power);
        psionicComp.PsionicPowersActionEntities.Add(power.Comp.ActionEntity);
        Dirty(power);
    }

    /// <summary>
    /// This is called whenever an entity pushes the psionic power action button.
    /// </summary>
    /// <param name="psionic">The psionic who attempts to use a psionic power.</param>
    /// <param name="args">The action event for said power.</param>
    private void OnPowerActionUsed(Entity<T> psionic, ref T1 args)
    {
        if (Timing.ApplyingState || args.Handled)
            return;

        args.Handled = true;

        if (Psionic.CanUsePsionicAbility(psionic))
        {
            OnPowerUsed(psionic, ref args);
            return;
        }

        Popup.PopupClient(Loc.GetString("psionic-cannot-use-psionics"), args.Performer, args.Performer);
    }

    /// <summary>
    /// This is the creme of the system. If you add a new power, you will want to call this.
    /// This is where the actual power takes place that follows after the button press.
    /// You will need to call LogPowerUsed() at the end.
    /// You will need to call Psionic.CanBeTargeted(target) if you add a power that can target things.
    /// IMPORTANT! The psionic entity DOES NOT HAVE TO BE THE PLAYER! It can be a clothing!
    /// If you want something to affect the player everytime, use args.Performer!
    /// psionic.Owner => Source of the power. args.Performer => The user of the power.
    /// </summary>
    /// <param name="psionic">The source of the power.</param>
    /// <param name="args">The event.</param>
    protected abstract void OnPowerUsed(Entity<T> psionic, ref T1 args);

    /// <summary>
    /// This handles being dispelled while having an active DoAfter.
    /// It'll be enough for most usages.
    /// But you can overwrite it if you have multiple DoAfters or require a specific interaction with being dispelled.
    /// </summary>
    /// <param name="psionic">The psionic who has been dispelled.</param>
    /// <param name="args">The DispelledEvent.</param>
    protected virtual void OnDispelled(Entity<T> psionic, ref DispelledEvent args)
    {
        if (args.Handled
            || psionic.Comp.GetDoAfterId() is not { } doAfterId)
            return;

        DoAfter.Cancel(doAfterId);
        Popup.PopupClient(Loc.GetString("psionic-dispelled"), args.Target, args.Target, PopupType.MediumCaution);
        psionic.Comp.RemoveSavedDoAfterId();

        args.Handled = true;
        Dirty(psionic);
    }

    /// <summary>
    /// This handles when the psionic gets mindbroken via chemicals or other.
    /// </summary>
    /// <param name="psionic">The psionic who is being mindbroken.</param>
    /// <param name="args">The mindbreaking event.</param>
    protected virtual void OnMindBroken(Entity<T> psionic, ref PsionicMindBrokenEvent args)
    {
        if (psionic.Comp.CanBeRemoved || args.Force)
        {
            args.Success = true;
            TryStopDoAfter(psionic, psionic.Owner); // Mindbreaking already has a popup, so no popup for breaking doAfters.
            Action.RemoveAction(psionic.Comp.ActionEntity);
            RemComp<T>(psionic);
            return;
        }

        args.AllRemoved = false;
    }

    /// <summary>
    /// This handles equipping gear that has psionic abilities. These will be automatically relayed to the user.
    /// </summary>
    /// <param name="psionicClothing">The clothing that grants the power and has the component.</param>
    /// <param name="args">The event.</param>
    private void OnGrantingClothingEquipped(Entity<T> psionicClothing, ref GetItemActionsEvent args)
    {
        if (args.SlotFlags is null or SlotFlags.POCKET
            || !HasComp<PotentialPsionicComponent>(args.User)) // IPCs and non-player organics shouldn't be able to use abilities.
            return;

        args.AddAction(psionicClothing.Comp.ActionEntity);
    }

    /// <summary>
    /// This interrupts DoAfters when being suppressed psionically.
    /// No putting on skullcaps AFTER casting your DoAfter to be secure from Dispels.
    /// </summary>
    /// <param name="power">The power source.</param>
    /// <param name="args">The event.</param>
    protected virtual void OnPsionicallySuppressed(Entity<T> power, ref PsionicSuppressedEvent args)
    {
        if (Timing.ApplyingState)
            return;

        TryStopDoAfter(power, args.Victim, "psionic-equipped-shielded-in-doafter");
    }

    /// <summary>
    /// This is raised when the suppression stops.
    /// This allows powers to handle special behavior.
    /// </summary>
    /// <param name="psionic">The psionic who is no longer suppressed.</param>
    /// <param name="args">The event.</param>
    protected virtual void OnStoppedPsionicallySuppressed(Entity<T> psionic, ref PsionicStoppedSuppressedEvent args)
    {
    }

    /// <summary>
    /// This will log the power usage and increase glimmer, as well as making sure metapsionics hear it.
    /// </summary>
    /// <param name="psionicSource">The SOURCE of the psionic power.</param>
    /// <param name="performer">The entity that PERFORMED the power.</param>
    protected void AfterPowerUsed(Entity<T> psionicSource, EntityUid performer)
    {
        var power = Loc.GetString(psionicSource.Comp.PowerName);
        _adminLogger.Add(Database.LogType.Psionics, Database.LogImpact.Medium, $"{ToPrettyString(psionicSource):player} used {power}");

        var ev = new PsionicPowerUsedEvent(performer, psionicSource, power);
        RaiseLocalEvent(psionicSource, ev);

        _glimmer.Glimmer += Random.Next(psionicSource.Comp.MinGlimmerChanged, psionicSource.Comp.MaxGlimmerChanged);
    }

    /// <summary>
    /// Stops the current DoAfter of the psionic user.
    /// </summary>
    /// <param name="psionic">The source of the power.</param>
    /// <param name="performer">The entity performing the power.</param>
    /// <param name="popup">The FTL string for the popup. If null, no popup.</param>
    /// <returns>Returns true if an doAfter was interrupted, otherwise false.</returns>
    private bool TryStopDoAfter(Entity<T> psionic, EntityUid performer, string? popup = null)
    {
        if (psionic.Comp.GetDoAfterId() is not { } doAfterId)
            return false;

        DoAfter.Cancel(doAfterId);
        if (popup != null)
            Popup.PopupClient(Loc.GetString(popup), performer, performer, PopupType.MediumCaution);

        psionic.Comp.RemoveSavedDoAfterId();
        Dirty(psionic);
        return true;
    }
}
