using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Verbs;

namespace Content.Shared._DV.Chapel;

public abstract class SharedSacrificialAltarSystem : EntitySystem
{
    [Dependency] private readonly SharedBuckleSystem _buckle = default!;
    [Dependency] protected readonly SharedDoAfterSystem DoAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SacrificialAltarComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<SacrificialAltarComponent, UnstrappedEvent>(OnUnstrapped);
        SubscribeLocalEvent<SacrificialAltarComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
    }

    private void OnExamined(Entity<SacrificialAltarComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("altar-examine"));
    }

    private void OnUnstrapped(Entity<SacrificialAltarComponent> ent, ref UnstrappedEvent args)
    {
        if (ent.Comp.DoAfter is {} id)
        {
            DoAfter.Cancel(id);
            ent.Comp.DoAfter = null;
        }
    }

    private void OnGetVerbs(Entity<SacrificialAltarComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
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
            Act = () => AttemptSacrifice(ent, user, target),
            Text = Loc.GetString("altar-sacrifice-verb"),
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

    protected virtual void AttemptSacrifice(Entity<SacrificialAltarComponent> ent, EntityUid user, EntityUid target)
    {
    }
}
