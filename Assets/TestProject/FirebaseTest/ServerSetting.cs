using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>서버 환경</summary>
public static class ServerSetting
{
    private static string _common;
    public static string commonUrl => _common ??= string.Format("http://scvcms-cdn.flerogamessvc.com/ab_nft/{0}", BuildSetting.type != EBuildType.Release ? "dev" : "live");

    public static string serverName { get; private set; } = "dev";
    public static EServerType serverType { get; private set; }
    public static EServerStatus status { get; private set; }
    public static string gameUrl { get; private set; }
    public static string cdnUrl { get; private set; }
    public static int useCoupon { get; private set; }
    public static string gameDataUrl { get; private set; }
    public static string resourceDataUrl { get; private set; }
    public static bool isEncryptServer { get; private set; }
    public static string sess;

    /// <summary>점검 가능 서버 여부</summary>
    /// 개발, 라이브 서버만 점검 체크하도록 예외처리
    public static bool isMaintenanceServer { get; private set; }

    public static Encryptor encryptor = new Encryptor("D(&Ww(zGl-z=m872+3x5o^CkpZQ*jNtT", "bJLrdMjwETwAYFGK");

    public static Dictionary<string, string> urls = new Dictionary<string, string>()
    {
        { "TermsOfService", "https://service.wemade-connect.com/policy/TermsofService.html" },      //이용약관
        { "PrivacyPolicy", "https://service.wemade-connect.com/policy/PrivacyPolicy_Google.html" }, //개인정보 처리방침        
        { "TermsOfService_EN", "https://service.wemade-connect.com/policy/TermsofService_EN.html" },
        { "PrivacyPolicy_EN", "https://service.wemade-connect.com/policy/PrivacyPolicy_EN.html" },
#if UNITY_ANDROID || UNITY_EDITOR
        { "Market", "https://play.google.com/store/apps/details?id=com.wemadeif.abyssmeta" },      //[Todo] 마켓 URL 등록
#elif UNITY_IOS
        { "Market", "https://www.naver.com" },
#endif
        { "OneLink", "https://abyssriumorigin.onelink.me/w8oy/v6yrpjbi" },  //[Todo] 원링크 URL 등록
#if UNITY_ANDROID
        { "AbyssriumMarket", "https://play.google.com/store/apps/details?id=com.idleif.abyssrium" },
#endif
#if BETA
        { "BetaResearch", "https://docs.google.com/forms/u/0/d/1rrZ-jfaThbX4L99Lu3fiqFRR4Ax5_RnG5Qa27Dg8_Bc/viewform?edit_requested=true" },
#endif
		{ "CCPA", "https://service.wemade-connect.com/policy/CCPA.html" } // 캘리포니아 거주 약관.
    };

    public static void Set(VersionData data)
    {
        sess = null;

        if (data == null)
        {
            status = EServerStatus.Update_Essential;
            return;
        }

        gameUrl = data.game_url;
        //cdnUrl = data.cdn_url;
        useCoupon = data.coupon_use;
        status = data.status;
        gameDataUrl = string.Empty;

        var i = data.cdn_url.LastIndexOf('/') + 1;
        cdnUrl = data.cdn_url.Substring(0, i);
        serverName = data.cdn_url.Substring(i, data.cdn_url.Length - i);
        gameDataUrl = string.Format("{0}gamedata/{1}", cdnUrl, serverName);
        resourceDataUrl = string.Format("{0}resources/{1}", cdnUrl, serverName);

        switch (serverName)
        {
            case "dev":
                serverType = EServerType.Dev;
                isMaintenanceServer = true;
                isEncryptServer = true;
                break;
            case "qa":
                serverType = EServerType.Qa;
                isMaintenanceServer = false;
                isEncryptServer = false;
                break;
            case "review":
                serverType = EServerType.Review;
                isMaintenanceServer = false;
                isEncryptServer = false;
                break;
            case "live":
                serverType = EServerType.Live;
                isMaintenanceServer = true;
                isEncryptServer = false;
                break;
            default:
                serverType = EServerType.Dev;
                isMaintenanceServer = true;
                isEncryptServer = false;
                break;
        }

        Debug.LogFormat("[Server/Setting] status {0}", status);
        Debug.LogFormat("[Server/Setting/Url] game {0}", gameUrl);
        Debug.LogFormat("[Server/Setting/Url] cdn {0}", cdnUrl);
        Debug.LogFormat("[Server/Setting/Url] table {0}", gameDataUrl);
        Debug.LogFormat("[Server/Setting/Url] resource {0}", resourceDataUrl);
    }
}