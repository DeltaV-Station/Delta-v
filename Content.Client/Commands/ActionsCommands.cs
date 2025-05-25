using Content.Client.Actions;
using System.IO;
using Content.Shared.Administration;
using Robust.Client.UserInterface;
using YamlDotNet.RepresentationModel;
using Robust.Shared.Console;

namespace Content.Client.Commands;

// Disabled until sandoxing issues are resolved. In the meantime, if you want to create an acttions preset, just disable
// sandboxing and uncomment this code (and the SaveActionAssignments() function).
/*
[AnyCommand]
public sealed class SaveActionsCommand : IConsoleCommand
{
    public string Command => "saveacts";
    public string Description => "Saves the current action toolbar assignments to a file";
    public string Help => $"Usage: {Command} <user resource path>";
    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteLine(Help);
            return;
        }

        try
        {
            EntitySystem.Get<ActionsSystem>().SaveActionAssignments(args[0]);
        }
        catch
        {
            shell.WriteLine("Failed to save action assignments");
        }
    }
}
*/

[AnyCommand]
public sealed class LoadActionsCommand : LocalizedCommands
{
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

    public override string Command => "loadacts";

    public override string Help => LocalizationManager.GetString($"cmd-{Command}-help", ("command", Command));

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            LoadActs(); // DeltaV - Load from a file dialogue instead
            return;
        }

        try
        {
            _entitySystemManager.GetEntitySystem<ActionsSystem>().LoadActionAssignments(args[0], true);
        }
        catch
        {
            shell.WriteError(LocalizationManager.GetString($"cmd-{Command}-error"));
        }
    }

    /// <summary>
    /// DeltaV - Load actions from a file stream instead
    /// </summary>
    private static async void LoadActs()
    {
        var fileMan = IoCManager.Resolve<IFileDialogManager>();
        var actMan = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<ActionsSystem>();

        var stream = await fileMan.OpenFile(new FileDialogFilters(new FileDialogFilters.Group("yml")));
        if (stream is null)
            return;

        var reader = new StreamReader(stream);
        var yamlStream = new YamlStream();
        yamlStream.Load(reader);

        actMan.LoadActionAssignments(yamlStream);
        reader.Close();
    }
}
