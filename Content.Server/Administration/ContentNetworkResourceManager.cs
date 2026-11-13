using Content.Server.Database;
using Content.Shared.CCVar;
using Robust.Server.Upload;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Upload;

namespace Content.Server.Administration;

public sealed class ContentNetworkResourceManager
{
    [Dependency] private readonly IServerDbManager _serverDb = default!;
    [Dependency] private readonly NetworkResourceManager _netRes = default!;
    [Dependency] private readonly IConfigurationManager _cfgManager = default!;

    [ViewVariables] public bool StoreUploaded { get; set; } = true;

    public void Initialize()
    {
        _cfgManager.OnValueChanged(CCVars.ResourceUploadingStoreEnabled, value => StoreUploaded = value, true);
        AutoDelete(_cfgManager.GetCVar(CCVars.ResourceUploadingStoreDeletionDays));
        _netRes.ResourcesUploaded += OnUploadResource;
    }

    private async void OnUploadResource(NetworkResourcesUploadedEvent msg)
    {
        if (StoreUploaded)
        {
            foreach(var file in msg.Files)
            {
                await _serverDb.AddUploadedResourceLogAsync(msg.Session.UserId, DateTime.Now, file.Relative.ToString(), file.Data);
            }
        }
    }

    private async void AutoDelete(int days)
    {
        if (days > 0)
            await _serverDb.PurgeUploadedResourceLogAsync(days);
    }
}
