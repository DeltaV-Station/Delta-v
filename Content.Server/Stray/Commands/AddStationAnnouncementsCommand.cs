using System.Linq;
using Content.Server.Audio;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Console;
using Robust.Shared.ContentPack;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Content.Server.Administration;
using Content.Server.Chat;
using Content.Server.Chat.Systems;
using Robust.Shared.Audio.Systems;
using Content.Server.Administration.Logs;
using Robust.Server.Audio;
using Content.Shared.Database;

namespace Content.Server.Administration.Commands;

[AdminCommand(AdminFlags.Fun)]
public sealed class AddStationAnnouncementsCommand : IConsoleCommand
{
    private List<string> allVars = new List<string>(){
        "Побег_Сингулярности",
        "Побег_Теслы",
        "Начало_Зомби_апокалепсиса",
        "Неизвестный_Объект",
        "Прибытие_ОБР",
        "Вылет_ПЦК",
        "Окончание_раунда_с_не_выполненной_миссией",
        "Окончание_раунда_с_выполненной_миссией",
        "Начало_Революции",
        "Запрос_ЭС",
        "Запрос_ОБР",
        "Вылет_ОБР_изза_ситуации_на_станции",
        "Несанкционированный_эвак",
        "Санкции_за_непослушание_ОЦК",
        "Запрос_РХБЗЗ"
    };
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly IResourceManager _res = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;

    public string Command => "add_station_announcement";
    public string Description => "Send an announcement and plays audio";
    public string Help => $"{Command} <announcement_audio> <announcement_text> to send station announcement.";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var chat = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<ChatSystem>();

        if (args.Length <= 1)
        {
            string ann = "";
            string aud = "";
            int danger = 0;
            switch(args[0]){
                case "Побег_Сингулярности":
                    ann = Loc.GetString("singularity-escape-announcement");
                    aud = "/Audio/Stray/Announcements/SingularityEscape.ogg";
                    danger = 2;
                    break;
                case "Побег_Теслы":
                    ann = Loc.GetString("tesla-escape-announcement");
                    aud = "/Audio/Stray/Announcements/TeslaEscape.ogg";
                    danger = 2;
                    break;
                case "Начало_Зомби_апокалепсиса":
                    ann = Loc.GetString("zombie-infection-begin-announcement");
                    aud = "/Audio/Stray/Announcements/ZombieInfectionStarted.ogg";
                    danger = 2;
                    break;
                case "Неизвестный_Объект":
                    ann = Loc.GetString("unknown-object-nearby-announcement");
                    aud = "/Audio/Stray/Announcements/ObjectNearby.ogg";
                    danger = 1;
                    break;
                case "Прибытие_ОБР":
                    ann = Loc.GetString("ert-shuttle-arrival-announcement");
                    aud = "/Audio/Stray/Announcements/ERTarrival.ogg";
                    danger = 0;
                    break;
                case "Вылет_ПЦК":
                    ann = Loc.GetString("centcom-representative-arrival-announcement");
                    aud = "/Audio/Stray/Announcements/CentComR.ogg";
                    danger = 0;
                    break;
                case "Окончание_раунда_с_не_выполненной_миссией":
                    ann = Loc.GetString("round-end-with-failed-mission-announcement");
                    aud = "/Audio/Stray/Announcements/MissionFail.ogg";
                    danger = 0;
                    break;
                case "Окончание_раунда_с_выполненной_миссией":
                    ann = Loc.GetString("round-end-with-finished-mission-announcement");
                    aud = "/Audio/Stray/Announcements/MissionComplete.ogg";
                    danger = 0;
                    break;
                case "Начало_Революции":
                    ann = Loc.GetString("revolution-start-announcement");
                    aud = "/Audio/Stray/Announcements/VivaLaRevolution.ogg";
                    danger = 2;
                    break;
                case "Запрос_ЭС":
                    ann = Loc.GetString("death-squad-call-announcement");
                    aud = "/Audio/Stray/Announcements/DeathSquad.ogg";
                    danger = 0;
                    break;
                case "Запрос_ОБР":
                    ann = Loc.GetString("ert-call-announcement");
                    aud = "/Audio/Stray/Announcements/pleaseERT.ogg";
                    danger = 0;
                    break;
                case "Вылет_ОБР_изза_ситуации_на_станции":
                    ann = Loc.GetString("ert-because-delta-code-announcement");
                    aud = "/Audio/Stray/Announcements/ERTcall.ogg";
                    danger = 2;
                    break;
                case "Несанкционированный_эвак":
                    ann = Loc.GetString("unsanctions-evac-announcement");
                    aud = "/Audio/Stray/Announcements/Evar.ogg";
                    danger = 0;
                    break;
                case "Санкции_за_непослушание_ОЦК":
                    ann = Loc.GetString("sanctions-announcement");
                    aud = "/Audio/Stray/Announcements/Fired.ogg";
                    danger = 0;
                    break;
                case "Запрос_РХБЗЗ":
                    ann = Loc.GetString("rhbzz-call-announcement");
                    aud = "/Audio/Stray/Announcements/RHBZZ.ogg";
                    danger = 0;
                    break;
                default:
                    shell.WriteError("No such event found");
                    _adminLogger.Add(LogType.EventAnnounced, $"{shell.Player} tries to add station announcement [{args[0]}] via command. but something went wrong");
                    return;
            }
            _adminLogger.Add(LogType.EventAnnounced, $"{shell.Player} add station announcement [{ann}] with audio [{aud}] via command");

            Color color = Color.Gold;

            switch(danger){
                case 0:
                    color = Color.Gold;
                    break;
                case 1:
                    color = Color.Red;
                    break;
                case 2:
                    color = Color.DarkRed;
                    break;
                default:
                    color = Color.Gold;
                    break;
            }

            chat.DispatchGlobalAnnouncement(ann, playSound: false, colorOverride: color);
            _entManager.System<AudioSystem>().PlayGlobal(aud, Filter.Broadcast(), true);
            shell.WriteLine("Sent!");
        }
        else if (args.Length == 3)
        {
            Color color = Color.Gold;
            if(int.TryParse(args[2], out var danger)){
                switch(danger){
                    case 0:
                        color = Color.Gold;
                        break;
                    case 1:
                        color = Color.Red;
                        break;
                    case 2:
                        color = Color.DarkRed;
                        break;
                    default:
                        color = Color.Gold;
                        break;
                }
            }else{
                _adminLogger.Add(LogType.EventAnnounced, $"{shell.Player} tries to add station announcement [{args[1]}] with audio [{args[0]}] with danger [{args[2]}] via command. but something went wrong");
                return;
            }
            //shell.WriteError("Too much arguments! Max: 2.");
            //return;
            _adminLogger.Add(LogType.EventAnnounced, $"{shell.Player} add station announcement [{args[1]}] with audio [{args[0]}] with danger [{args[2]}] via command");
            chat.DispatchGlobalAnnouncement(Loc.GetString(args[1]), playSound: false, colorOverride: color);
            _entManager.System<AudioSystem>().PlayGlobal(args[0], Filter.Broadcast(), true);

            shell.WriteLine("Sent!");
        }else{
            string err = "";
            foreach(string ar in args){
                err += ar +"; ";
            }
            _adminLogger.Add(LogType.EventAnnounced, $"{shell.Player} tries to add station announcement with arguments[{err}] via command. but something went wrong");

            shell.WriteError("Not enough/too much arguments!");
            return;
        }
    }
    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {

        if (args.Length == 1)
        {
            if(args[0] != ""&&args[0][0] == '/'){
                var hint = "path to announcement audio";

                var options = CompletionHelper.AudioFilePath(args[0], _protoManager, _res);

                return CompletionResult.FromHintOptions(options, hint);
            }else if(args[0] == ""||(args[0] != ""&&args[0][0] != '/')){
                var hint = "<event_name>";
                var options = allVars;
                //var options = CompletionHelper.PrototypeIDs(args[0], _protoManager, _res);

                return CompletionResult.FromHintOptions(options, hint);
            }
        }
        if (args.Length == 2&&args[0][0] == '/')
        {
            return CompletionResult.FromHint("path to announcement text");
        }
        if (args.Length == 3&&args[0][0] == '/')
        {
            var options = new List<string>(){"0", "1", "2"};
            return CompletionResult.FromHintOptions(options, "danger");
        }
        return CompletionResult.Empty;
    }
}
