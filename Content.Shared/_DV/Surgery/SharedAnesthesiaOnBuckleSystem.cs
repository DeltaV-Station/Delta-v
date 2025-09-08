using Content.Shared.Buckle.Components;
using Content.Shared.Examine;

namespace Content.Shared._DV.Surgery
{
    public sealed class SharedStabilizeOnBuckleSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<AnesthesiaComponent, StrappedEvent>(OnStrapped);
            SubscribeLocalEvent<AnesthesiaComponent, UnstrappedEvent>(OnUnstrapped);
            SubscribeLocalEvent<AnesthesiaComponent, ExaminedEvent>(OnExamine);
        }

        private void OnStrapped(Entity<AnesthesiaComponent> bed, ref StrappedEvent args)
        {
            EnsureComp<AnesthesiaComponent>(args.Buckle.Owner);
        }

        private void OnUnstrapped(Entity<AnesthesiaComponent> bed, ref UnstrappedEvent args)
        {
            RemComp<AnesthesiaComponent>(args.Buckle.Owner);
        }

        private void OnExamine(Entity<AnesthesiaComponent> ent, ref ExaminedEvent args)
        {
            if (!HasComp<StrapComponent>(ent))
            {
                return;
            }
            args.PushMarkup(Loc.GetString("anesthesia-on-buckle"));
        }
    }
}
