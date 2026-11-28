using Content.Server.RoundEnd;
using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Localizations;
using Robust.Shared.Console;

namespace Content.Server._DV.Administration.Commands
{
    [AdminCommand(AdminFlags.Round)]
    public sealed class DVCallShuttleNoRecallCommand : LocalizedEntityCommands
    {
        [Dependency] private readonly RoundEndSystem _roundEndSystem = default!;

        public override string Command => "callshuttlenorecall";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            switch (args.Length)
            {
                case 0:
                    // No arguments = default shuttle call
                    _roundEndSystem.RequestRoundEnd(shell.Player?.AttachedEntity, checkCooldown: false, cantRecall: true);
                    break;
                case 1:
                    // One argument = time passed in
                    if (!TimeSpan.TryParseExact(args[0], ContentLocalizationManager.TimeSpanMinutesFormats, LocalizationManager.DefaultCulture, out var timeSpan))
                    {
                        shell.WriteLine(Loc.GetString("shell-timespan-minutes-must-be-correct"));
                        return;
                    }

                    _roundEndSystem.RequestRoundEnd(timeSpan, shell.Player?.AttachedEntity, checkCooldown: false, cantRecall: true);
                    break;
                default:
                    // More than one = error
                    shell.WriteLine(Loc.GetString("shell-wrong-arguments-number"));
                    break;
            }
            // No arguments = default shuttle call
            if (args.Length == 0) {
                _roundEndSystem.RequestRoundEnd(shell.Player?.AttachedEntity, checkCooldown: false, cantRecall: true);
                return;
            }
        }
    }
}
