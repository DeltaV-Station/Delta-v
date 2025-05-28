using Content.Shared._DV.DogWhistle.Components;
using Content.Shared._DV.DogWhistle.Events;
using Content.Shared.Actions;
using Content.Shared.Inventory.Events;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._DV.DogWhistle.EntitySystems;

/// <summary>
/// Shared logic for the Dog whistle order system
/// </summary>
public abstract class SharedDogWhistleSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] protected readonly SharedPopupSystem PopupSystem = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private readonly LocId _whistleToggleBase = "dog-whistle-toggle-action";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DogWhistleComponent, ToggleDogWhistleEvent>(OnWhistleToggled);
        SubscribeLocalEvent<DogWhistleComponent, GetItemActionsEvent>(OnGetWhistleActions);
        SubscribeLocalEvent<DogWhistleComponent, GotUnequippedEvent>(OnWhistleUnequipped);
    }

    /// <summary>
    /// Handles when a player uses the toggle action for the whistle and updates the state.
    /// </summary>
    /// <param name="whistle">Whistle that was toggled.</param>
    /// <param name="args">Args for the event, notably the performer.</param>
    private void OnWhistleToggled(Entity<DogWhistleComponent> whistle, ref ToggleDogWhistleEvent args)
    {
        if (whistle.Comp.ToggleActionEntid == null || _timing.ApplyingState)
            return;

        if (!TryComp<InstantActionComponent>(whistle.Comp.ToggleActionEntid, out var actionComp))
            return;

        var newState = !actionComp.Toggled;

        string msg;
        if (newState)
        {
            EnsureComp<DogWhistleHolderComponent>(args.Performer, out var holderComp);
            holderComp.Whistle = whistle;
            msg = $"{_whistleToggleBase}-up";
        }
        else
        {
            RemComp<DogWhistleHolderComponent>(args.Performer);
            msg = $"{_whistleToggleBase}-down";
        }

        if (_net.IsServer)
        {
            PopupSystem.PopupEntity(Loc.GetString(msg, ("user", args.Performer)), args.Performer, args.Performer);
        }

        args.Toggle = true;
        args.Handled = true;
    }

    /// <summary>
    /// Handles when a whistle is equipped and the inventory system queries for any actions associated with it.
    /// </summary>
    /// <param name="whistle">Whistle that was equipped.</param>
    /// <param name="args">Args for the event.</param>
    private void OnGetWhistleActions(Entity<DogWhistleComponent> whistle, ref GetItemActionsEvent args)
    {
        args.AddAction(ref whistle.Comp.ToggleActionEntid, whistle.Comp.ToggleAction);
        Dirty(whistle);
    }

    /// <summary>
    /// Handles when a whistle is unequipped by an entity, clearing up any toggles and other components.
    /// </summary>
    /// <param name="whistle">Whistle that was unequipped.</param>
    /// <param name="args">Args for the event, notably who unequipped it.</param>
    private void OnWhistleUnequipped(Entity<DogWhistleComponent> whistle, ref GotUnequippedEvent args)
    {
        // Ensure the action and equipee are cleaned up
        if (whistle.Comp.ToggleActionEntid != null)
            _actionsSystem.SetToggled(whistle.Comp.ToggleActionEntid, false);

        RemComp<DogWhistleHolderComponent>(args.Equipee);
    }
}
