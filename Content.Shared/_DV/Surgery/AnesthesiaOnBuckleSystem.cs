using Content.Shared.Buckle.Components;
using Content.Shared.Examine;

namespace Content.Shared._DV.Surgery;

public sealed class AnesthesiaOnBuckleSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AnesthesiaOnBuckleComponent, StrappedEvent>(OnStrapped);
        SubscribeLocalEvent<AnesthesiaOnBuckleComponent, UnstrappedEvent>(OnUnstrapped);
        SubscribeLocalEvent<AnesthesiaOnBuckleComponent, ExaminedEvent>(OnExamine);
    }
    private void OnStrapped(Entity<AnesthesiaOnBuckleComponent> strap, ref StrappedEvent args)
    {
        strap.Comp.HadAnesthesia = HasComp<AnesthesiaComponent>(args.Buckle.Owner);
        EnsureComp<AnesthesiaComponent>(args.Buckle.Owner);
    }
    private void OnUnstrapped(Entity<AnesthesiaOnBuckleComponent> strap, ref UnstrappedEvent args)
    {
        if (!strap.Comp.HadAnesthesia)
            RemComp<AnesthesiaComponent>(args.Buckle.Owner);
    }
    private void OnExamine(Entity<AnesthesiaOnBuckleComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("anesthesia-on-buckle"));
    }
}
