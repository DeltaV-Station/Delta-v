using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

namespace Content.Shared.Stray.Utilitys.LogMessageSender;

public abstract class SharedLogMessageSenderSystem : EntitySystem
{
    public virtual void LogError(string message){
        Log.Error(message);
    }
    public virtual void LogWarn(string message){
        Log.Warning(message);
    }
    public virtual void LogInfo(string message){
        Log.Info(message);
    }
    public virtual void LogFatal(string message){
        Log.Fatal(message);
    }
    public virtual void LogDebug(string message){
        Log.Debug(message);
    }
    public virtual void LogVerbose(string message){
        Log.Verbose(message);
    }
}

