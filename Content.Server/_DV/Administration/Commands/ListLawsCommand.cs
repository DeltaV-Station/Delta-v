using Content.Server.Administration;
using Content.Server.Commands;
using Content.Server.Preferences.Managers;
using Content.Shared.Administration;
using Content.Shared.Mind;
using Content.Shared.Silicons.Laws.Components;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Administration.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class ListLawsCommand : LocalizedEntityCommands
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;


    public override string Command => "lslaws";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length > 1)
        {
            shell.WriteLine(Help);
            return;
        }

        switch (args.Length)
        {
            case 0:
                foreach (var (ent, lawProvider) in _entityManager.AllEntities<SiliconLawProviderComponent>())
                {
                    WriteLawReport(shell, ent, lawProvider);
                }
                break;
            case 1:
                if (!_player.TryGetSessionByUsername(args[0], out var session) || !_entityManager.TryGetComponent<SiliconLawProviderComponent>(session.AttachedEntity, out var provider))
                {
                    shell.WriteError(Loc.GetString("cmd-lslaws-error-bad-player"));
                    return;
                }

                WriteLawReport(shell, session.AttachedEntity.Value, provider);
                break;
            default:
                shell.WriteLine(Help);
                break;
        }


    }

    private void WriteLawReport(IConsoleShell shell, EntityUid ent, SiliconLawProviderComponent lawProvider)
    {
        shell.WriteLine("");
        _entityManager.TryGetComponent<MetaDataComponent>(ent, out var metaData);
        var entityName = metaData?.EntityName;

        shell.WriteMarkup(_player.TryGetSessionByEntity(ent, out var session)
            ? $"[bold]{entityName}[/bold] ({ent.Id}, [color=red]{session.Name}[/color], subverted: {lawProvider.Subverted})"
            : $"[bold]{entityName}[/bold] ({ent.Id}, subverted: {lawProvider.Subverted})");

        shell.WriteLine($"Base Lawset: {lawProvider.Laws.Id}");
        if (lawProvider.Lawset is {} lawset)
        {
            foreach (var siliconLaw in lawset.Laws)
            {
                shell.WriteLine($"{siliconLaw.Order}: {Loc.GetString(siliconLaw.LawString)}");
            }
        }
        else
        {
            shell.WriteLine("Unable to retrieve laws.");
        }
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length == 1 ? CompletionResult.FromOptions(CompletionHelper.SessionNames()) : CompletionResult.Empty;
    }
}
