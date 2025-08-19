using Content.Shared.Access.Systems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
<<<<<<< HEAD
using Robust.Shared.GameObjects;
=======
using Robust.Shared.Timing;
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30

namespace Content.Shared._DV.SmartFridge;

public sealed class SmartFridgeSystem : EntitySystem
{
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SmartFridgeComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<SmartFridgeComponent, EntRemovedFromContainerMessage>(OnItemRemoved);

        Subs.BuiEvents<SmartFridgeComponent>(SmartFridgeUiKey.Key,
            sub =>
            {
                sub.Event<SmartFridgeDispenseItemMessage>(OnDispenseItem);
            });
    }

<<<<<<< HEAD
=======
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SmartFridgeComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.Ejecting || _timing.CurTime <= comp.EjectEnd)
                continue;
            comp.EjectEnd = null;
            Dirty(uid, comp);
        }
    }

    /// <summary>
    /// Attempts to insert an item into a SmartFridge, checked against its item whitelist.
    /// Optionally checks user access, if a user is passed in, displaying an error in-game if they don't have access.
    /// </summary>
    /// <param name="ent">The SmartFridge being inserted into</param>
    /// <param name="item">The item being inserted</param>
    /// <param name="user">The user who should be access-checked</param>
    /// <param name="container">The SmartFridge's container if it's already known</param>
    /// <returns>Whether the insertion was successful</returns>
    public bool TryAddItem(Entity<SmartFridgeComponent?> ent,
        EntityUid item,
        EntityUid? user = null,
        BaseContainer? container = null)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return false;

        if (container == null && !_container.TryGetContainer(ent, ent.Comp.Container, out container))
            return false;

        if (!_whitelist.CheckBoth(item, ent.Comp.Blacklist, ent.Comp.Whitelist))
            return false;

        if (user != null && !Allowed((ent, ent.Comp), user.Value))
            return false;

        _container.Insert(item, container);
        var key = new SmartFridgeEntry(Identity.Name(item, EntityManager));

        ent.Comp.Entries.Add(key);

        ent.Comp.ContainedEntries.TryAdd(key, []);
        ent.Comp.ContainedEntries[key].Add(GetNetEntity(item));

        Dirty(ent, ent.Comp);
        return true;
    }

    public void TryAddItem(Entity<SmartFridgeComponent?> ent,
        IEnumerable<EntityUid> items,
        EntityUid? user = null,
        BaseContainer? container = null)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        if (container == null && !_container.TryGetContainer(ent, ent.Comp.Container, out container))
            return;

        if (user != null && !Allowed((ent, ent.Comp), user.Value))
            return;

        foreach (var item in items)
        {
            // Don't pass the user since we've already checked access
            TryAddItem(ent, item, null, container);
        }
    }

>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
    private void OnInteractUsing(Entity<SmartFridgeComponent> ent, ref InteractUsingEvent args)
    {
        if (!_hands.CanDrop(args.User, args.Used))
            return;

        if (!TryAddItem(ent!, args.Used, args.User))
            return;

        _audio.PlayPredicted(ent.Comp.InsertSound, ent, args.User);
    }

    private void OnItemRemoved(Entity<SmartFridgeComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        var key = new SmartFridgeEntry(Identity.Name(args.Entity, EntityManager));

        if (ent.Comp.ContainedEntries.TryGetValue(key, out var contained))
        {
            contained.Remove(GetNetEntity(args.Entity));
        }

        Dirty(ent);
    }

    private bool Allowed(Entity<SmartFridgeComponent> machine, EntityUid user)
    {
        if (_accessReader.IsAllowed(user, machine))
            return true;

        _popup.PopupPredicted(Loc.GetString("smart-fridge-component-try-eject-access-denied"), machine, user);
        _audio.PlayPredicted(machine.Comp.SoundDeny, machine, user);
        return false;
    }

    private void OnDispenseItem(Entity<SmartFridgeComponent> ent, ref SmartFridgeDispenseItemMessage args)
    {
        if (!Allowed(ent, args.Actor))
            return;

        if (!ent.Comp.ContainedEntries.TryGetValue(args.Entry, out var contained))
        {
            _audio.PlayPredicted(ent.Comp.SoundDeny, ent, args.Actor);
            _popup.PopupPredicted(Loc.GetString("smart-fridge-component-try-eject-unknown-entry"), ent, args.Actor);
            return;
        }

        foreach (var item in contained)
        {
            if (!_container.TryRemoveFromContainer(GetEntity(item)))
                continue;

            _audio.PlayPredicted(ent.Comp.SoundVend, ent, args.Actor);
            contained.Remove(item);
            Dirty(ent);
            return;
        }

        _audio.PlayPredicted(ent.Comp.SoundDeny, ent, args.Actor);
        _popup.PopupPredicted(Loc.GetString("smart-fridge-component-try-eject-out-of-stock"), ent, args.Actor);
    }
}
