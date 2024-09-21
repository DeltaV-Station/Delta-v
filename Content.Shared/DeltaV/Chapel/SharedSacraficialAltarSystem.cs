using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Verbs;

namespace Content.Shared.DeltaV.Chapel;

public abstract class SharedSacraficialAltarSystem : EntitySystem
{
    [Dependency] private readonly SharedBuckleSystem _buckle = default!;
    [Dependency] protected readonly SharedDoAfterSystem DoAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SacraficialAltarComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<SacraficialAltarComponent, UnstrappedEvent>(OnUnstrapped);
        SubscribeLocalEvent<SacraficialAltarComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
    }

    private void OnExamined(Entity<SacraficialAltarComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("altar-examine"));
    }

    private void OnUnstrapped(Entity<SacraficialAltarComponent> ent, ref UnstrappedEvent args)
    {
        if (ent.Comp.DoAfter is {} id)
        {
            DoAfter.Cancel(id);
            ent.Comp.DoAfter = null;
        }
    }

    private void OnGetVerbs(Entity<SacraficialAltarComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || ent.Comp.DoAfter != null)
            return;

        if (!TryComp<StrapComponent>(ent, out var strap))
            return;

        if (GetFirstBuckled(strap) is not {} target)
            return;

        var user = args.User;
        args.Verbs.Add(new AlternativeVerb()
        {
            Act = () => AttemptSacrafice(ent, user, target),
            Text = Loc.GetString("altar-sacrafice-verb"),
            Priority = 2
        });
    }

    private EntityUid? GetFirstBuckled(StrapComponent strap)
    {
        foreach (var entity in strap.BuckledEntities)
        {
            return entity;
        }

        return null;
    }

    protected virtual void AttemptSacrafice(Entity<SacraficialAltarComponent> ent, EntityUid user, EntityUid target)
    {
    }
}
