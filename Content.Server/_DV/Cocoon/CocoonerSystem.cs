using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Server.Speech.Components;
using Content.Shared._DV.Cocoon;
using Content.Shared.Administration.Logs;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.Stunnable;
using Robust.Shared.Containers;
using Robust.Shared.Random;

namespace Content.Server._DV.Cocoon
{
    public sealed class CocooningSystem : EntitySystem
    {
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly DoAfterSystem _doAfter = default!;
        [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
        [Dependency] private readonly BlindableSystem _blindableSystem = default!;
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
        [Dependency] private readonly SharedDestructibleSystem _destructibleSystem = default!;
        [Dependency] private readonly IRobustRandom _robustRandom = default!;

        private const string BodySlot = "body_slot";

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<CocoonComponent, EntInsertedIntoContainerMessage>(OnCocEntInserted);
            SubscribeLocalEvent<CocoonComponent, EntRemovedFromContainerMessage>(OnCocEntRemoved);
        }

        private void OnCocEntInserted(EntityUid uid, CocoonComponent component, EntInsertedIntoContainerMessage args)
        {
            component.Victim = args.Entity;

            if (TryComp<ReplacementAccentComponent>(args.Entity, out var currentAccent))
                component.OldAccent = currentAccent.Accent;

            EnsureComp<ReplacementAccentComponent>(args.Entity).Accent = "mumble";
            EnsureComp<StunnedComponent>(args.Entity);

            _blindableSystem.UpdateIsBlind(args.Entity);
        }

        private void OnCocEntRemoved(EntityUid uid, CocoonComponent component, EntRemovedFromContainerMessage args)
        {
            if (TryComp<ReplacementAccentComponent>(args.Entity, out var replacement))
                if (component.OldAccent is not null)
                    replacement.Accent = component.OldAccent;
                else
                    RemComp(args.Entity, replacement);


            RemComp<StunnedComponent>(args.Entity);
            _blindableSystem.UpdateIsBlind(args.Entity);
        }
    }
}
