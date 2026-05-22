using System.Linq;
using Robust.Shared.Console;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Content.Shared.Administration;
using Content.Server.Administration;
using Content.Server._DV.Mail.EntitySystems;
using Content.Shared._DV.Mail;
using Robust.Shared.Timing;

namespace Content.Server._DV.Mail;

[AdminCommand(AdminFlags.Fun)]
public sealed class MailToCommand : LocalizedEntityCommands
{
    public override string Command => "mailto";
    public override string Description => Loc.GetString("cmd-mailto-description", ("requiredComponent", nameof(MailReceiverComponent)));
    public override string Help => Loc.GetString("cmd-mailto-help", ("command", Command));

    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly MailSystem _mail = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedMailSystem _sharedMail = default!;

    private static readonly EntProtoId BlankMailPrototype = "MailAdminFun";
    private static readonly EntProtoId BlankLargeMailPrototype = "MailLargeAdminFun";
    private const string Container = "storagebase";
    private const string MailContainer = "contents";

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 =>
                CompletionResult.FromHintOptions(
                    CompletionHelper.Components<MailReceiverComponent>(args[0], EntityManager),
                    Loc.GetString("cmd-mailto-hint-recipient")),
            2 =>
                CompletionResult.FromHintOptions(CompletionHelper.NetEntities(args[1], EntityManager),
                    Loc.GetString("cmd-mailto-hint-container")),
            3 =>
                CompletionResult.FromHintOptions(CompletionHelper.Booleans,
                    Loc.GetString("cmd-mailto-hint-fragile")),
            4 =>
                CompletionResult.FromHintOptions(CompletionHelper.Booleans,
                    Loc.GetString("cmd-mailto-hint-priority")),
            5 =>
                CompletionResult.FromHintOptions(CompletionHelper.Booleans, Loc.GetString("cmd-mailto-hint-large")),
            _ => CompletionResult.Empty
        };
    }

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 4)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
            return;
        }

        if (!EntityUid.TryParse(args[0], out var recipientUid) || !EntityUid.TryParse(args[1], out var containerUid))
        {
            shell.WriteError(Loc.GetString("shell-entity-uid-must-be-number"));
            return;
        }

        if (!bool.TryParse(args[2], out var isFragile) || !bool.TryParse(args[3], out var isPriority))
        {
            shell.WriteError(Loc.GetString("shell-invalid-bool"));
            return;
        }

        var isLarge = false;
        if (args.Length > 4 && !bool.TryParse(args[4], out isLarge))
        {
            shell.WriteError(Loc.GetString("shell-invalid-bool"));
            return;
        }
        var mailPrototype = isLarge ? BlankLargeMailPrototype : BlankMailPrototype;

        if (!EntityManager.HasComponent<MailReceiverComponent>(recipientUid))
        {
            shell.WriteLine(Loc.GetString("cmd-mailto-no-mailreceiver", ("requiredComponent", nameof(MailReceiverComponent))));
            return;
        }

        if (!_prototype.HasIndex<EntityPrototype>(mailPrototype))
        {
            shell.WriteLine(Loc.GetString("cmd-mailto-no-blankmail", ("blankMail", mailPrototype)));
            return;
        }

        if (!_container.TryGetContainer(containerUid, Container, out var targetContainer))
        {
            shell.WriteLine(Loc.GetString("cmd-mailto-invalid-container", ("requiredContainer", Container)));
            return;
        }

        if (!_sharedMail.TryGetMailRecipientForReceiver(recipientUid, out var recipient))
        {
            shell.WriteLine(Loc.GetString("cmd-mailto-unable-to-receive"));
            return;
        }

        if (!_sharedMail.TryGetMailTeleporterForReceiver(recipientUid, out var teleporterComponent, out var teleporterUid))
        {
            shell.WriteLine(Loc.GetString("cmd-mailto-no-teleporter-found"));
            return;
        }

        var mailUid = EntityManager.SpawnEntity(mailPrototype, EntityManager.GetComponent<TransformComponent>(containerUid).Coordinates);
        var mailContents = _container.EnsureContainer<Container>(mailUid, MailContainer);

        if (!EntityManager.TryGetComponent<MailComponent>(mailUid, out var mailComponent))
        {
            shell.WriteLine(Loc.GetString("cmd-mailto-bogus-mail", ("blankMail", mailPrototype), ("requiredMailComponent", nameof(MailComponent))));
            return;
        }

        foreach (var entity in targetContainer.ContainedEntities.ToArray())
        {
            _container.Insert(entity, mailContents);
        }

        _sharedMail.SetFragile((mailUid, mailComponent), isFragile);
        _sharedMail.SetPriority((mailUid, mailComponent), isPriority);
        _sharedMail.SetLarge((mailUid, mailComponent), isLarge);

        _mail.SetupMail(mailUid, teleporterComponent, recipient.Value);

        var teleporterQueue = _container.EnsureContainer<Container>((EntityUid)teleporterUid, "queued");
        _container.Insert(mailUid, teleporterQueue);
        shell.WriteLine(Loc.GetString("cmd-mailto-success", ("timeToTeleport", teleporterComponent.NextDelivery - _timing.CurTime)));
    }
}

[AdminCommand(AdminFlags.Fun)]
public sealed class MailNowCommand : LocalizedEntityCommands
{
    public override string Command => "mailnow";
    public override string Description => Loc.GetString("cmd-mailnow");
    public override string Help => Loc.GetString("cmd-mailnow-help", ("command", Command));

    [Dependency] private readonly MailSystem _mail = default!;

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var query = EntityManager.EntityQueryEnumerator<MailTeleporterComponent>();
        while (query.MoveNext(out var uid, out var mailTeleporter))
        {
            _mail.DeliverNow((uid, mailTeleporter));
        }

        shell.WriteLine(Loc.GetString("cmd-mailnow-success"));
    }
}
