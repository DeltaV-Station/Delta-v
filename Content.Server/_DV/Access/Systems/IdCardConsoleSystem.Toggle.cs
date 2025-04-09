using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Database;
using System.Linq;

namespace Content.Server.Access.Systems;

public sealed partial class IdCardConsoleSystem
{
    private void InitializeToggle()
    {
        Subs.BuiEvents<IdCardConsoleComponent>(IdCardConsoleComponent.IdCardConsoleUiKey.Key, subs =>
        {
            subs.Event<IdCardConsoleToggleMessage>(OnToggle);
        });
    }

    private void OnToggle(Entity<IdCardConsoleComponent> ent, ref IdCardConsoleToggleMessage args)
    {
        if (ent.Comp.TargetIdSlot.Item is not {} targetId ||
            ent.Comp.PrivilegedIdSlot.Item is not {} privilegedId ||
            !PrivilegedIdIsAuthorized(ent, ent) ||
            // malf client
            !_prototype.HasIndex(args.Id))
        {
            return;
        }

        // malf client
        var user = args.Actor;
        if (!ent.Comp.AccessLevels.Contains(args.Id))
        {
            _sawmill.Warning($"User {ToPrettyString(user)} tried to write unknown access tag.");
            return;
        }

        if (!TryComp<AccessComponent>(targetId, out var access))
            return;

        // malf client
        var privilegedPerms = _accessReader.FindAccessTags(privilegedId).ToHashSet();
        if (!privilegedPerms.Contains(args.Id))
            return;

        var adding = !access.Tags.Contains(args.Id);
        if (adding)
            access.Tags.Add(args.Id);
        else
            access.Tags.Remove(args.Id);
        Dirty(targetId, access);

        var verb = adding ? "added" : "removed";
        var prefix = adding ? "to" : "from";
        // TODO: only log changes when ejecting the id
        _adminLogger.Add(LogType.Action, LogImpact.Medium,
            $"{ToPrettyString(user):user} has {verb} access level '{args.Id}' {prefix} {ToPrettyString(targetId):entity}");
    }
}
