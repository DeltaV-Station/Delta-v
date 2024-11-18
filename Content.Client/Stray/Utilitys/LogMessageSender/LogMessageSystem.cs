using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Content.Shared.Stray.Utilitys.PopupOnInteract;
using Content.Shared.Stray.Utilitys.LogMessageSender;

namespace Content.Client.Stray.Utilitys.LogMessageSender;

public sealed class LogMessageSenderSystem : SharedLogMessageSenderSystem
{
    public override void LogError(string message){
        Log.Error(message);
    }
    public override void LogWarn(string message){
        Log.Warning(message);
    }
    public override void LogInfo(string message){
        Log.Info(message);
    }
    public override void LogFatal(string message){
        Log.Fatal(message);
    }
    public override void LogDebug(string message){
        Log.Debug(message);
    }
    public override void LogVerbose(string message){
        Log.Verbose(message);
    }
}
