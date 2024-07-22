using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Content.Server.Administration.Managers;
using Content.Server.Database;
using Content.Server.DeltaV.ProxyDetection.NeutrinoApi;
using Content.Shared.Database;
using Content.Shared.DeltaV.CCVars;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Network;

namespace Content.Server.DeltaV.ProxyDetection;

public sealed class ProxyDetectionManager : IPostInjectInit
{
    [Dependency] private readonly IServerDbManager _dbManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly ILocalizationManager _loc = default!;
    [Dependency] private readonly ILogManager _log = default!;
    [Dependency] private readonly IBanManager _banManager = default!;

    private ISawmill _sawmill = default!;

    private bool _shouldProbe;
    private bool _apiValid;

    private NeutrinoApiClient _neutrinoApiClient = default!;

    public async Task<(bool, string)> ShouldDeny(NetConnectingArgs e)
    {
        var addr = e.IP.Address;

        if (IPAddress.IsLoopback(addr))
            return (false, "");

        var existingProxy = await _dbManager.GetServerBanAsync(addr, null, null);

        if (existingProxy != null)
            return (false, "");

        if (!_apiValid) // API is invalid, cancel
            return (false, "");

        var blacklistParameters = new Dictionary<string, string>
        {
            { "ip", addr.ToString() },
            { "vpn-lookup", "true" }
        };
        var probeParameters = new Dictionary<string, string>
        {
            { "ip", addr.ToString()}
        };

        var blacklistResponse = await _neutrinoApiClient.IpBlocklist(blacklistParameters);
        if (!blacklistResponse.IsOk())
        {
            _sawmill.Error("API Error: {0}, Error Code: {1}, HTTP Status Code: {2}",
                blacklistResponse.ErrorMessage,
                blacklistResponse.ErrorCode.ToString(),
                blacklistResponse.HttpStatusCode.ToString()); // you should handle this gracefully!
            _sawmill.Error($"{blacklistResponse.ErrorCause}");
            return (false, "");
        }

        var data = blacklistResponse.Data;
        data.TryGetProperty("blocklists", out var blockList);
        data.TryGetProperty("is-listed", out var isListed);

        if (!isListed.GetBoolean() && !_shouldProbe)
            return (false, "");

        string result;

        if (!isListed.GetBoolean() && _shouldProbe)
        {
            var probeResponse = await _neutrinoApiClient.IpProbe(probeParameters);
            if (!probeResponse.IsOk())
            {
                _sawmill.Error("API Error: {0}, Error Code: {1}, HTTP Status Code: {2}",
                    probeResponse.ErrorMessage,
                    probeResponse.ErrorCode.ToString(),
                    probeResponse.HttpStatusCode.ToString()); // you should handle this gracefully!
                _sawmill.Error($"{probeResponse.ErrorCause}");
                return (false, "");
            }

            var probeData = probeResponse.Data;
            probeData.TryGetProperty("is-proxy", out var isProxy);
            probeData.TryGetProperty("is-hosting", out var isHosting);
            probeData.TryGetProperty("is-vpn", out var isVpn);
            if (!isProxy.GetBoolean() && !isHosting.GetBoolean() && !isVpn.GetBoolean())
                return (false, "");

            result = "Suspicious connection.";
        }
        else
        {
            var blockListsArray = blockList.EnumerateArray().Select(item => item.GetString()).ToList();
            var blockLists = string.Join(", ", blockListsArray);

            result = $"Your address was found in the following blacklists: {blockLists}";
        }

        var hid = addr.AddressFamily == AddressFamily.InterNetworkV6 ? 128 : 32;
        _banManager.CreateServerBan(e.UserId, e.UserName, null, (addr, hid), null, null, NoteSeverity.High, result,
            ServerBanExemptFlags.Datacenter);
        return (true, result);
    }

    void IPostInjectInit.PostInject()
    {
        _sawmill = _log.GetSawmill("PROXYDETECTION");

        if (_cfg.GetCVar(DCCVars.BlockProxyConnections))
        {
            if (_cfg.GetCVar(DCCVars.ProxyApiKey) == "" || _cfg.GetCVar(DCCVars.ProxyApiUser) == "")
                return;

            _neutrinoApiClient =
                new NeutrinoApiClient(_cfg.GetCVar(DCCVars.ProxyApiUser), _cfg.GetCVar(DCCVars.ProxyApiKey));

            _apiValid = true; // TODO: Should probably actually validate this
            _shouldProbe = _cfg.GetCVar(DCCVars.ProxyProbe);
        }
    }
}
