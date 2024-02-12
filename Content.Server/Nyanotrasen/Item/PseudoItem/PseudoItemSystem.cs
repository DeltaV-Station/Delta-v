using Content.Shared.IdentityManagement;
using Content.Shared.Nyanotrasen.Item.PseudoItem;
using Content.Shared.Storage;
using Content.Shared.Verbs;

namespace Content.Server.Nyanotrasen.Item.PseudoItem;

public sealed class PseudoItemSystem : SharedPseudoItemSystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<PseudoItemComponent, GetVerbsEvent<AlternativeVerb>>(AddInsertAltVerb);
    }

    // For whatever reason, I have to put these in server or the verbs duplicate
    private void AddInsertAltVerb(EntityUid uid, PseudoItemComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        if (component.Active)
            return;

        if (!TryComp<StorageComponent>(args.Using, out _))
            return;

        // There *should* be a check here to see if we can fit, but I'm not aware of an easy way to do that, so eh, who cares

        if (args.Hands?.ActiveHandEntity == null)
            return;

        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                StartInsertDoAfter(args.User, uid, args.Hands.ActiveHandEntity.Value, component);
            },
            Text = Loc.GetString("action-name-insert-other", ("target", Identity.Entity(args.Target, EntityManager))),
            Priority = 2
        };
        args.Verbs.Add(verb);
    }
}
