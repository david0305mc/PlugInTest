using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Linq;
using Request = Protocols.Request;
using Response = Protocols.Response;

public static class ServerAPI 
{

    /// <summary>서버 정보 요청</summary>
    public static async UniTask<EServerStatus> GetServerStatus(CancellationToken cancellationToken = default)
    {
        var serverVersions = await UnityHttp.Get<VersionData[]>(string.Format("{0}/version_list.json", ServerSetting.commonUrl), cancellationToken: cancellationToken);
        var versionData = serverVersions.FirstOrDefault(x => x.os == ServerSetting.GetOSCode() && x.version == BuildSetting.version);
        ServerSetting.Set(versionData);
        return ServerSetting.status;
    }
    public static async UniTask<Response.SignIn> SignIn(
           EPlatform platform,
           string platformId,
           string lang,
           string push_id,
           CancellationToken cancellationToken = default)
    {
        var data = new Request.SignIn();
        data.platform = (int)platform;
        data.platform_id = platformId;
        data.lang = lang;
        data.push_id = push_id;
#if !UNITY_EDITOR && UNITY_ANDROID
        data.os = 1;
#elif !UNITY_EDITOR && UNITY_IOS
        data.os = 2;
#else
        data.os = 1;
#endif

#if USE_GPRESTO
        data.gpresto_engine_state = GPrestoManager.Instance.EngineState;
        data.gpresto_sdata = GPrestoManager.Instance.CrossCheckData;
#endif

        ResponseContext responseData = await UnityHttp.Send(RequestContext.Create(ServerCmd.AUTH_USER_LOGIN, data,
            defaultRetryHandling: false,
            defaultExceptionHandling: false), cancellationToken);
        GameTime.Init(responseData.server_time);

        var signInData = responseData.GetResult<Response.SignIn>();
        if (signInData.leave_time > 0)
        {
            //탈퇴 유예중인 유저 예외발생
            //throw new LeaveUserException(signInData);
        }

        //userData.country = signInData.country;

#if UNITY_EDITOR && ENABLE_SERVER
        PlayerPrefs.SetString(ZString.Format("{0}/UserToken", ServerSetting.serverName), signInData.platform_id);
#endif
        return signInData;
    }

    public static async UniTask<Response.Login> Login(ulong uno, string token, CancellationToken cancellationToken = default)
    {
        var data = new Request.Login();
        data.uno = uno;
        data.token = token;

        var responseData = await UnityHttp.Send<Response.Login>(RequestContext.Create(ServerCmd.AUTH_GAME_LOGIN, data,
            defaultRetryHandling: false,
            defaultExceptionHandling: false), cancellationToken);
        //userData.isLogin = true;
        //userData.uno = uno;
        ServerSetting.sess = responseData.session;
        Debug.Log("Session : " + responseData.session);
        Debug.Log("uNO : " + uno);

        return responseData;
    }

    public static void Logout(bool force = false)
    {
        //AuthManager.Instance.Logout(force);
        //MessageDispather.Publish(EMessage.User_Leave);
        //UnityEngine.SceneManagement.SceneManager.LoadScene("intro");
    }


    public static async UniTask Exception(UnityHttpException e, CancellationToken cancellationToken = default)
    {
        if (e is UnityHttpNetworkException)
        {
            await Exception((UnityHttpNetworkException)e, cancellationToken);
        }
        //else if (e is UnityHttpGameServerException)
        //{
        //    await Exception((UnityHttpGameServerException)e, cancellationToken);
        //}
        //else if (e is UnityHttpGameServerMaintenance)
        //{
        //    await Exception((UnityHttpGameServerMaintenance)e, cancellationToken);
        //}

        throw e;
    }
}

