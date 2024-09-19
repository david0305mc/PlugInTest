using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>서버 환경</summary>
public static class ServerSetting
{
    private static string _common;
#if ENABLE_SERVER
    public static string commonUrl => _common ??= string.Format("http://scvcms-cdn.flerogamessvc.com/ab_nft/{0}", BuildSetting.type != EBuildType.Release ? "dev" : "live");
#else
    public static string commonUrl => _common ??= string.Format("http://scvcms-cdn.flerogamessvc.com/ab_classic/{0}", BuildSetting.type != EBuildType.Release ? "dev" : "live");
#endif

#if RELEASE
    public static string serverName { get; private set; } = "live";
#else
    public static string serverName { get; private set; } = "dev";
#endif
    public static EServerType serverType { get; private set; }
    public static EServerStatus status { get; private set; }
    public static string gameUrl { get; private set; }
    public static string cdnUrl { get; private set; }
    public static bool useCoupon { get; private set; }
    public static string gameDataUrl { get; private set; }
    public static string resourceDataUrl { get; private set; }
    public static bool isEncryptServer { get; private set; }
    public static string sess;
    public static int lastAppVersion; //서버에 등록된 가장 최신의 앱 버전    

    /// <summary>점검 가능 서버 여부</summary>
    /// 개발, 라이브 서버만 점검 체크하도록 예외처리
    public static bool isMaintenanceServer { get; private set; }

    public static Encryptor encryptor = new Encryptor("D(&Ww(zGl-z=m872+3x5o^CkpZQ*jNtT", "bJLrdMjwETwAYFGK");

    public static Dictionary<string, string> urls = new Dictionary<string, string>()
    {
        { "TermsOfService", "https://service.wemade-connect.com/policy/TermsofService.html" },      //이용약관
        { "TermsOfService_EN", "https://service.wemade-connect.com/policy/TermsofService_EN.html" },
        { "PrivacyPolicy", "https://service.wemade-connect.com/policy/PrivacyPolicy_Google.html" }, //개인정보 처리방침                
        { "PrivacyPolicy_EN", "https://service.wemade-connect.com/policy/PrivacyPolicy_EN.html" },
        { "AgreePolicy", "https://service.wemade-connect.com/policy/agreePrivacy_KR.html" },
        { "AgreePolicy_EN", "https://service.wemade-connect.com/policy/agreePrivacy_EN.html" },

#if UNITY_ANDROID || UNITY_EDITOR
        { "MarketTerms", "https://play.google.com/intl/en/about/play-terms/index.html" },
#elif UNITY_IOS
        { "MarketTerms", "https://www.apple.com/legal/internet-services/itunes/dev/stdeula/" },
#endif

#if UNITY_ANDROID || UNITY_EDITOR
        { "SubscriptionGuide", "https://support.google.com/googleplay/answer/7018481?hl=en&co=GENIE.Platform%3DAndroid" },
#elif UNITY_IOS
        { "SubscriptionGuide", "https://support.apple.com/en-us/HT202039" },
#endif

#if UNITY_ANDROID || UNITY_EDITOR
        { "Market", "https://play.google.com/store/apps/details?id=com.wemadeconnect.abyssclassic" },
#elif UNITY_IOS
        { "Market", "https://itunes.apple.com/app/id6476528044" },
#endif
        { "OneLink", "https://abyssclassic.onelink.me/Z3tO/rtalli38" },
#if UNITY_ANDROID
        { "AbyssriumMarket", "https://play.google.com/store/apps/details?id=com.wemadeconnect.abyssclassic" },
#endif
#if BETA
        { "BetaResearch", "https://docs.google.com/forms/u/0/d/1rrZ-jfaThbX4L99Lu3fiqFRR4Ax5_RnG5Qa27Dg8_Bc/viewform?edit_requested=true" },
#endif
		{ "CCPA", "https://service.wemade-connect.com/policy/CCPA.html" }, // 캘리포니아 거주 약관.
        { "ZenDesk", "https://flerogames.zendesk.com/hc/ko/requests/new?tf_360007006771=abyssclassic" },
    };

    static ServerSetting()
    {
        var data = new VersionData();
#if DEV
        //data.game_url = "https://abyss-classic-dev.wemade-connect.com/server.php";
        data.game_url = "http://13.209.180.168/server.php";
        data.cdn_url = "https://dlh9pjezq3ww.cloudfront.net/dev";
#else
        data.game_url = "https://abyss-classic-live.wemade-connect.com/server.php";
        data.cdn_url = "https://dlh9pjezq3ww.cloudfront.net/live";
#endif

        data.coupon_use = 0;
        Set(data);
    }
    public static OSCode GetOSCode()
    {
#if UNITY_ANDROID
        return OSCode.Android;
#elif UNITY_IOS
        return OSCode.iOS;
#else
        return OSCode.Android; 
#endif
    }
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
        useCoupon = data.coupon_use > 0;
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
                isEncryptServer = true;
                break;
            case "review":
                serverType = EServerType.Review;
                isMaintenanceServer = false;
                isEncryptServer = true;
                break;
            case "live":
                serverType = EServerType.Live;
                isMaintenanceServer = true;
                isEncryptServer = true;
                break;
            default:
                serverType = EServerType.Dev;
                isMaintenanceServer = true;
                isEncryptServer = true;
                break;
        }

        Debug.LogFormat("[Server/Setting] status {0}", status);
        Debug.LogFormat("[Server/Setting/Url] game {0}", gameUrl);
        Debug.LogFormat("[Server/Setting/Url] cdn {0}", cdnUrl);
        Debug.LogFormat("[Server/Setting/Url] table {0}", gameDataUrl);
        Debug.LogFormat("[Server/Setting/Url] resource {0}", resourceDataUrl);
    }
}