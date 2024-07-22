using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Content.Server.DeltaV.ProxyDetection.NeutrinoApi;

/// <summary>Make a request to the Neutrino API</summary>
public sealed class NeutrinoApiClient
{
    /// <summary>MulticloudEndpoint server</summary>
    private const string Endpoint = "https://neutrinoapi.net/";

    private readonly string _baseUrl;

    private static readonly HttpClient Client = new();
    private const int DefaultTimeoutInSeconds = 300;

    /// <summary>NeutrinoApiClient constructor</summary>
    public NeutrinoApiClient(string userId, string apiKey)
    {
        _baseUrl = Endpoint;
        Client.DefaultRequestHeaders.Add("User-ID", userId);
        Client.DefaultRequestHeaders.Add("Api-Key", apiKey);
        Client.Timeout = TimeSpan.FromSeconds(DefaultTimeoutInSeconds);
    }

    /// <summary>NeutrinoApiClient constructor using base URL override</summary>
    public NeutrinoApiClient(string userId, string apiKey, string baseUrl)
    {
        _baseUrl = baseUrl;
        Client.DefaultRequestHeaders.Add("User-ID", userId);
        Client.DefaultRequestHeaders.Add("Api-Key", apiKey);
        Client.Timeout = TimeSpan.FromSeconds(DefaultTimeoutInSeconds);
    }

    /// <summary>
    ///     Check the reputation of an IP address, domain name or URL against a comprehensive list of blacklists and
    ///     blocklists
    /// </summary>
    /// <link>https://www.neutrinoapi.com/api/host-reputation</link>
    /// <returns>Returns an ApiResponse object on success or failure</returns>
    public async Task<ApiResponse> HostReputation(Dictionary<string, string> paramDict)
    {
        return await ExecRequest("GET", "host-reputation", paramDict, 120);
    }

    /// <summary>The IP Blocklist API will detect potentially malicious or dangerous IP addresses</summary>
    /// <link>https://www.neutrinoapi.com/api/ip-blocklist</link>
    /// <returns>Returns an ApiResponse object on success or failure</returns>
    public async Task<ApiResponse> IpBlocklist(Dictionary<string, string> paramDict)
    {
        return await ExecRequest("GET", "ip-blocklist", paramDict, 10);
    }

    /// <summary>Get location information about an IP address and do reverse DNS (PTR) lookups</summary>
    /// <link>https://www.neutrinoapi.com/api/ip-info</link>
    /// <returns>Returns an ApiResponse object on success or failure</returns>
    public async Task<ApiResponse> IpInfo(Dictionary<string, string> paramDict)
    {
        return await ExecRequest("GET", "ip-info", paramDict, 10);
    }

    /// <summary>Execute a realtime network probe against an IPv4 or IPv6 address</summary>
    /// <link>https://www.neutrinoapi.com/api/ip-probe</link>
    /// <returns>Returns an ApiResponse object on success or failure</returns>
    public async Task<ApiResponse> IpProbe(Dictionary<string, string> paramDict)
    {
        return await ExecRequest("GET", "ip-probe", paramDict, 120);
    }

    /// <summary>
    ///     Make a request to the Neutrino API
    /// </summary>
    /// <param name="httpMethod"></param>
    /// <param name="endpoint"></param>
    /// <param name="paramDict"></param>
    /// <param name="outputFilePath"></param>
    /// <param name="timeoutInSeconds"></param>
    /// <returns>ApiResponse object on success or failure</returns>
    private async Task<ApiResponse> ExecRequest(string httpMethod, string endpoint,
        Dictionary<string, string> paramDict, int timeoutInSeconds)
    {
        try
        {
            using var encodedParams = new FormUrlEncodedContent(paramDict);
            string url;
            if (httpMethod.Equals("GET"))
            {
                var paramStr = await encodedParams.ReadAsStringAsync();
                url = $"{_baseUrl}{endpoint}?{paramStr}";
            }
            else
                url = $"{_baseUrl}{endpoint}";

            using var request = new HttpRequestMessage();
            request.Method = new HttpMethod(httpMethod.Equals("POST") ? "POST" : "GET");
            request.RequestUri = new Uri(url);
            request.Content = httpMethod.Equals("POST") ? encodedParams : null;
            using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutInSeconds));
            using var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
                cancellationToken.Token);
            var content = await response.Content.ReadAsStringAsync(cancellationToken.Token);
            var statusCode = (int) response.StatusCode;
            var contentType = response.Content.Headers.ContentType?.ToString();

            ApiResponse apiResponse;

            if (response.IsSuccessStatusCode && contentType != null)
            {
                if (contentType.Contains("application/json"))
                {
                    var jsonStr = content;
                    apiResponse = ApiResponse.OfData(statusCode, contentType,
                        JsonSerializer.Deserialize<JsonElement>(jsonStr));
                }
                else
                {
                    apiResponse =
                        ApiResponse.OfHttpStatus(statusCode, contentType, ApiErrorCode.ApiGatewayError, content);
                }
            }
            else
            {
                if (contentType != null && contentType.Contains("application/json"))
                {
                    var json = JsonSerializer.Deserialize<JsonElement>(content);
                    json.TryGetProperty("api-error", out var errorCodeElement);
                    json.TryGetProperty("api-error-msg", out var errorMessageElement);
                    apiResponse = ApiResponse.OfHttpStatus(statusCode, contentType,
                        int.Parse(errorCodeElement.ToString() ?? "0"), errorMessageElement.ToString() ?? "");
                }
                else
                {
                    apiResponse = ApiResponse.OfHttpStatus(statusCode, contentType ?? "", ApiErrorCode.ApiGatewayError,
                        content);
                }
            }

            return apiResponse;
        }
        catch (IOException? e)
        {
            return ApiResponse.OfErrorCause(ApiErrorCode.NetworkIoError, e);
        }
        catch (FormatException? e)
        {
            return ApiResponse.OfErrorCause(ApiErrorCode.UrlParsingError, e);
        }
        catch (HttpRequestException? e)
        {
            return ApiResponse.OfErrorCause(ApiErrorCode.NetworkIoError, e);
        }
        catch (ArgumentException? e)
        {
            return ApiResponse.OfErrorCause(ApiErrorCode.BadUrl, e);
        }
        catch (AggregateException e)
        {
            foreach (var ie in e.Flatten().InnerExceptions)
            {
                switch (ie)
                {
                    case TaskCanceledException:
                        return ApiResponse.OfErrorCause(ApiErrorCode.ReadTimeout, ie);
                    case HttpRequestException:
                        return ApiResponse.OfErrorCause(ApiErrorCode.TlsProtocolError, ie);
                }
            }
        }

        return default!;
    }
}
