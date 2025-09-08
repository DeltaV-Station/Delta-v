using Content.Shared.Buckle.Components;
using Content.Shared.Examine;

namespace Content.Shared._DV.Surgery
{
    public sealed class SharedAnesthesiaOnBuckleSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<AnesthesiaOnBuckleComponent, StrappedEvent>(OnStrapped);
            SubscribeLocalEvent<AnesthesiaOnBuckleComponent, UnstrappedEvent>(OnUnstrapped);
            SubscribeLocalEvent<AnesthesiaOnBuckleComponent, ExaminedEvent>(OnExamine);
        }

        private void OnStrapped(Entity<AnesthesiaOnBuckleComponent> bed, ref StrappedEvent args)
        {
            EnsureComp<AnesthesiaComponent>(args.Buckle.Owner);
        }

        private void OnUnstrapped(Entity<AnesthesiaOnBuckleComponent> bed, ref UnstrappedEvent args)
        {
            RemComp<AnesthesiaComponent>(args.Buckle.Owner);
        }

        private void OnExamine(Entity<AnesthesiaOnBuckleComponent> ent, ref ExaminedEvent args)
        {
            if (!HasComp<StrapComponent>(ent))
            {
                return;
            }
            args.PushMarkup(Loc.GetString("anesthesia-on-buckle"));
        }
    }
}
