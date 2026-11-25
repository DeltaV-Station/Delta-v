using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Interaction;
using Content.Shared.Popups;

namespace Content.Shared._DV.Access;

public sealed class ReverseAgentIDCardSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ReverseAgentIDCardComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<ReverseAgentAccessConfiguratorComponent, AfterInteractEvent>(OnAccessReader);
    }

    private void OnAfterInteract(Entity<ReverseAgentIDCardComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Target == null || !args.CanReach || !TryComp<AccessComponent>(args.Target, out var targetAccess))
            return;

        if (!TryComp<AccessComponent>(ent, out var access) || !HasComp<IdCardComponent>(ent))
            return;

        if (ent.Comp.Overwrite)
        {
            targetAccess.Tags.Clear();
            targetAccess.Tags.UnionWith(access.Tags);
            _popup.PopupClient(Loc.GetString("reverse-agent-access-overwrote"), args.User, args.User);
        }
        else
        {
            targetAccess.Tags.UnionWith(access.Tags);
            _popup.PopupClient(Loc.GetString("reverse-agent-access-added"), args.User, args.User);
        }

        Dirty(ent, access);
    }

    private void OnAccessReader(Entity<ReverseAgentAccessConfiguratorComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Target == null || !args.CanReach || !TryComp<AccessReaderComponent>(args.Target, out var targetAccess))
            return;

        if (!TryComp<AccessReaderComponent>(ent, out var access))
            return;

        _accessReader.SetDenyTags((args.Target.Value, targetAccess), access.DenyTags);
        _accessReader.TrySetAccesses((args.Target.Value, targetAccess), access.AccessLists);
        _accessReader.SetAccessKeys((args.Target.Value, targetAccess), access.AccessKeys);

        _popup.PopupClient(Loc.GetString("reverse-agent-access-overwrote"), args.User, args.User);
        Dirty(ent, access);
    }
}
