namespace Content.Server.DeltaV.ProxyDetection.NeutrinoApi;

/// <summary>
///     Neutrino API error codes
/// </summary>
public sealed class ApiErrorCode
{
    /// <summary>InvalidParameter</summary>
    public const int InvalidParameter = 1;

    /// <summary>MaxCallLimit</summary>
    public const int MaxCallLimit = 2;

    /// <summary>BadUrl</summary>
    public const int BadUrl = 3;

    /// <summary>AbuseDetected</summary>
    public const int AbuseDetected = 4;

    /// <summary>NotResponding</summary>
    public const int NotResponding = 5;

    /// <summary>Concurrent</summary>
    public const int Concurrent = 6;

    /// <summary>NotVerified</summary>
    public const int NotVerified = 7;

    /// <summary>TelephonyLimit</summary>
    public const int TelephonyLimit = 8;

    /// <summary>InvalidJson</summary>
    public const int InvalidJson = 9;

    /// <summary>AccessDenied</summary>
    public const int AccessDenied = 10;

    /// <summary>MaxPhoneCalls</summary>
    public const int MaxPhoneCalls = 11;

    /// <summary>BadAudio</summary>
    public const int BadAudio = 12;

    /// <summary>HlrLimitReached</summary>
    public const int HlrLimitReached = 13;

    /// <summary>TelephonyBlocked</summary>
    public const int TelephonyBlocked = 14;

    /// <summary>TelephonyRateExceeded</summary>
    public const int TelephonyRateExceeded = 15;

    /// <summary>FreeLimit</summary>
    public const int FreeLimit = 16;

    /// <summary>RenderingFailed</summary>
    public const int RenderingFailed = 17;

    /// <summary>DeprecatedApi</summary>
    public const int DeprecatedApi = 18;

    /// <summary>CreditLimitReached</summary>
    public const int CreditLimitReached = 19;

    /// <summary>NotMultiEnabled</summary>
    public const int NotMultiEnabled = 21;

    /// <summary>NoBatchMode</summary>
    public const int NoBatchMode = 22;

    /// <summary>BatchLimitExceeded</summary>
    public const int BatchLimitExceeded = 23;

    /// <summary>BatchInvalid</summary>
    public const int BatchInvalid = 24;

    /// <summary>UserDefinedDailyLimit</summary>
    public const int UserDefinedDailyLimit = 31;

    /// <summary>AccessForbidden</summary>
    public const int AccessForbidden = 43;

    /// <summary>RequestTooLarge</summary>
    public const int RequestTooLarge = 44;

    /// <summary>NoEndpoint</summary>
    public const int NoEndpoint = 45;

    /// <summary>InternalServerError</summary>
    public const int InternalServerError = 51;

    /// <summary>ServerOffline</summary>
    public const int ServerOffline = 52;

    /// <summary>ConnectTimeout</summary>
    public const int ConnectTimeout = 61;

    /// <summary>ReadTimeout</summary>
    public const int ReadTimeout = 62;

    /// <summary>Timeout</summary>
    public const int Timeout = 63;

    /// <summary>DnsLookupFailed</summary>
    public const int DnsLookupFailed = 64;

    /// <summary>TlsProtocolError</summary>
    public const int TlsProtocolError = 65;

    /// <summary>UrlParsingError</summary>
    public const int UrlParsingError = 66;

    /// <summary>NetworkIoError</summary>
    public const int NetworkIoError = 67;

    /// <summary>FileIoError</summary>
    public const int FileIoError = 68;

    /// <summary>InvalidJsonResponse</summary>
    public const int InvalidJsonResponse = 69;

    /// <summary>NoData</summary>
    public const int NoData = 70;

    /// <summary>ApiGatewayError</summary>
    public const int ApiGatewayError = 71;

    /// <summary>
    ///     Get description of error code
    /// </summary>
    /// <param name="errorCode"></param>
    /// <returns></returns>
    public static string GetErrorMessage(int errorCode)
    {
        switch (errorCode)
        {
            case InvalidParameter:
                return "MISSING OR INVALID PARAMETER";
            case MaxCallLimit:
                return "DAILY API LIMIT EXCEEDED";
            case BadUrl:
                return "INVALID URL";
            case AbuseDetected:
                return "ACCOUNT OR IP BANNED";
            case NotResponding:
                return "NOT RESPONDING. RETRY IN 5 SECONDS";
            case Concurrent:
                return "TOO MANY CONNECTIONS";
            case NotVerified:
                return "ACCOUNT NOT VERIFIED";
            case TelephonyLimit:
                return "TELEPHONY NOT ENABLED ON YOUR ACCOUNT. PLEASE CONTACT SUPPORT FOR HELP";
            case InvalidJson:
                return "INVALID JSON. JSON CONTENT TYPE SET BUT NON-PARSABLE JSON SUPPLIED";
            case AccessDenied:
                return "ACCESS DENIED. PLEASE CONTACT SUPPORT FOR ACCESS TO THIS API";
            case MaxPhoneCalls:
                return "MAXIMUM SIMULTANEOUS PHONE CALLS";
            case BadAudio:
                return "COULD NOT LOAD AUDIO FROM URL";
            case HlrLimitReached:
                return "HLR LIMIT REACHED. CARD DECLINED";
            case TelephonyBlocked:
                return "CALLS AND SMS TO THIS NUMBER ARE LIMITED";
            case TelephonyRateExceeded:
                return "CALL IN PROGRESS";
            case FreeLimit:
                return "FREE PLAN LIMIT EXCEEDED";
            case RenderingFailed:
                return "RENDERING FAILED. COULD NOT GENERATE OUTPUT FILE";
            case DeprecatedApi:
                return "THIS API IS DEPRECATED. PLEASE USE THE LATEST VERSION";
            case CreditLimitReached:
                return "MAXIMUM ACCOUNT CREDIT LIMIT REACHED. PAYMENT METHOD DECLINED";
            case NotMultiEnabled:
                return "BATCH PROCESSING NOT ENABLED FOR THIS ENDPOINT";
            case NoBatchMode:
                return "BATCH PROCESSING NOT AVAILABLE ON YOUR PLAN";
            case BatchLimitExceeded:
                return "BATCH PROCESSING REQUEST LIMIT EXCEEDED";
            case BatchInvalid:
                return "INVALID BATCH REQUEST. DOES NOT CONFORM TO SPEC";
            case UserDefinedDailyLimit:
                return "DAILY API LIMIT EXCEEDED. SET BY ACCOUNT HOLDER";
            case AccessForbidden:
                return "ACCESS DENIED. USER ID OR API KEY INVALID";
            case RequestTooLarge:
                return "REQUEST TOO LARGE. MAXIMUM SIZE IS 5MB FOR DATA AND 25MB FOR UPLOADS";
            case NoEndpoint:
                return "ENDPOINT DOES NOT EXIST";
            case InternalServerError:
                return "FATAL EXCEPTION. REQUEST COULD NOT BE COMPLETED";
            case ServerOffline:
                return "SERVER OFFLINE. MAINTENANCE IN PROGRESS";
            case ConnectTimeout:
                return "TIMEOUT OCCURRED CONNECTING TO SERVER";
            case ReadTimeout:
                return "TIMEOUT OCCURRED READING API RESPONSE";
            case Timeout:
                return "TIMEOUT OCCURRED DURING API REQUEST";
            case DnsLookupFailed:
                return "ERROR RECEIVED FROM YOUR DNS RESOLVER";
            case TlsProtocolError:
                return "ERROR DURING TLS PROTOCOL HANDSHAKE";
            case UrlParsingError:
                return "ERROR PARSING REQUEST URL";
            case NetworkIoError:
                return "IO ERROR DURING API REQUEST";
            case FileIoError:
                return "IO ERROR WRITING TO OUTPUT FILE";
            case InvalidJsonResponse:
                return "INVALID JSON DATA RECEIVED";
            case NoData:
                return "NO PAYLOAD DATA RECEIVED";
            case ApiGatewayError:
                return "API GATEWAY ERROR";
            default:
                return $"Api Error {errorCode.ToString()}";
        }
    }
}
