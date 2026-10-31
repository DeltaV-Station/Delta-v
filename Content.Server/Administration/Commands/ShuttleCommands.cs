using Content.Server.RoundEnd;
using Content.Shared.Administration;
using Content.Shared.Localizations;
using Robust.Shared.Console;

namespace Content.Server.Administration.Commands
{
    [AdminCommand(AdminFlags.Round)]
    public sealed class CallShuttleCommand : LocalizedEntityCommands
    {
        [Dependency] private readonly RoundEndSystem _roundEndSystem = default!;

        public override string Command => "callshuttle";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            // DeltaV Start - Add support for cantRecall argument to the callshuttle command.
            // No arguments = default shuttle call
            if (args.Length == 0) {
                _roundEndSystem.RequestRoundEnd(shell.Player?.AttachedEntity, checkCooldown: false);
                return;
            }

            // One or more arguments means the time always has to be parsed
            if (!TimeSpan.TryParseExact(args[0], ContentLocalizationManager.TimeSpanMinutesFormats, LocalizationManager.DefaultCulture, out var timeSpan)) {
                shell.WriteLine(Loc.GetString("shell-timespan-minutes-must-be-correct"));
                return;
            }

            // We are done when there is only one argument. This is the upstream behavior of this command.
            if (args.Length == 1) {
                _roundEndSystem.RequestRoundEnd(timeSpan, shell.Player?.AttachedEntity, checkCooldown: false);
                return;
            }

            // If there are more than 2 arguments, we error out
            if (args.Length != 2)
                shell.WriteLine(Loc.GetString("shell-wrong-arguments-number"));

            // Parse the final argument as a bool and use it to decide if the shuttle can be recalled by the station or not.
            if (bool.TryParse(args[1], out var cantRecall))
                _roundEndSystem.RequestRoundEnd(timeSpan, shell.Player?.AttachedEntity, checkCooldown: false, cantRecall: cantRecall);
            else
                shell.WriteLine(Loc.GetString("shell-boolean-must-be-correct"));
            // DeltaV End
        }
    }

    [AdminCommand(AdminFlags.Round)]
    public sealed class RecallShuttleCommand : LocalizedEntityCommands
    {
        [Dependency] private readonly RoundEndSystem _roundEndSystem = default!;

        public override string Command => "recallshuttle";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            _roundEndSystem.CancelRoundEndCountdown(shell.Player?.AttachedEntity, forceRecall: true);
        }
    }
}
