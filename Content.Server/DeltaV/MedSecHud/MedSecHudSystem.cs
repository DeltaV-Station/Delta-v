using Content.Shared.Actions;
using Content.Shared.DeltaV.MedSecHud;
using Content.Shared.Clothing;
using Content.Shared.Overlays;
using Robust.Shared.GameStates;

namespace Content.Server.DeltaV.MedSecHud
{
    public sealed class MedSecHudSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly IEntityManager _entities = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<MedSecHudComponent, ClothingGotEquippedEvent>(OnEquip);
            SubscribeLocalEvent<MedSecHudComponent, ClothingGotUnequippedEvent>(OnUnequip);
            SubscribeLocalEvent<MedSecHudComponent, ToggleMedSecHudEvent>(OnToggle);
        }

        private void OnEquip(EntityUid uid, MedSecHudComponent component, ClothingGotEquippedEvent args)
        {
            _actions.AddAction(args.Wearer, ref component.ActionEntity, component.ActionId, uid);
            UpdateVisuals(uid, component);
        }

        private void OnUnequip(EntityUid uid, MedSecHudComponent component, ClothingGotUnequippedEvent args)
        {
            _actions.RemoveAction(args.Wearer, component.ActionEntity);
        }

        private void OnToggle(EntityUid uid, MedSecHudComponent component, ToggleMedSecHudEvent args)
        {

            component.MedicalMode = !component.MedicalMode;
            UpdateVisuals(uid, component);
        }

        private void UpdateVisuals(EntityUid uid, MedSecHudComponent component)
        {
            if (component.MedicalMode)
            {
                EnsureComp<ShowHealthBarsComponent>(uid);
                EnsureComp<ShowHealthIconsComponent>(uid);
                RemComp<ShowJobIconsComponent>(uid);
                RemComp<ShowMindShieldIconsComponent>(uid);
                RemComp<ShowCriminalRecordIconsComponent>(uid);
            }
            else
            {
                EnsureComp<ShowJobIconsComponent>(uid);
                EnsureComp<ShowMindShieldIconsComponent>(uid);
                EnsureComp<ShowCriminalRecordIconsComponent>(uid);
                RemComp<ShowHealthIconsComponent>(uid);
                RemComp<ShowHealthBarsComponent>(uid);
            }
            Dirty(uid, component);
        }
    }
}
