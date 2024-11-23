using Content.Shared.DeltaV.MedSecHud;
using Content.Shared.Clothing;

namespace Content.Server.DeltaV.MedSecHud
{
    public sealed class MedSecHudSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<MedSecHudComponent, ClothingGotEquippedEvent>(OnEquip);
            SubscribeLocalEvent<MedSecHudComponent, ToggleMedSecHudEvent>(OnToggle);
        }

        private void OnEquip(Entity<MedSecHudComponent> ent, ref ClothingGotEquippedEvent args)
        {
            UpdateVisuals(ent);
        }

        private void OnToggle(Entity<MedSecHudComponent> ent, ref ToggleMedSecHudEvent args)
        {
            ent.Comp.MedicalMode = !ent.Comp.MedicalMode;
            UpdateVisuals(ent);
        }

        private void UpdateVisuals(Entity<MedSecHudComponent> ent)
        {
            if (ent.Comp.MedicalMode)
            {
                EntityManager.AddComponents(ent, ent.Comp.AddComponents);
                EntityManager.RemoveComponents(ent, ent.Comp.RemoveComponents);
            }
            else
            {
                EntityManager.RemoveComponents(ent, ent.Comp.AddComponents); // just to confuse everyone
                EntityManager.AddComponents(ent, ent.Comp.RemoveComponents);
            }
            Dirty(ent);
        }
    }
}
