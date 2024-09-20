using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Linq;
using Request = Protocols.Request;
using Response = Protocols.Response;
using System;
using Newtonsoft.Json;

public class RawData
{
    public string tableName;
    public int date_key;
    public string save_data;
}

public class SaveData
{
    public ulong ver;
    public List<RawData> save_datas;
}

public static class ServerAPI
{
    public static ulong Uno { get; set; }
    private static bool useCompress = false;
    public static Dictionary<string, int> tableIdx = new Dictionary<string, int>()
    {
        {"BaseData", 1},

        {"InventoryData", 2},
    };
    private static Dictionary<int, string> _tableNames;
    public static Dictionary<int, string> tableNames
    {
        get
        {
            if (_tableNames == null)
                _tableNames = tableIdx.ToDictionary(x => x.Value, x => x.Key);

            return _tableNames;
        }
    }
    public static string ConvertToTableName(int idx)
    {
        return tableNames.GetValueOrDefault(idx);
    }

    public static int ConvertToDateKey(string tableName)
    {
        return tableIdx.GetValueOrDefault(tableName);
    }

    /// <summary>서버 정보 요청</summary>
    public static async UniTask<EServerStatus> GetServerStatus(bool exceptionThrow = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var datas = await UnityHttp.Get<VersionData[]>(string.Format("{0}/version_list.json", ServerSetting.commonUrl),
                defaultRetryHandling: false,
                defaultExceptionHandling: false,
                cancellationToken: cancellationToken);

            if (datas != null && datas.Length > 0)
            {
                var orderedEnumerable = datas.OrderByDescending(d => d.version);

                //서버에 해당 버전 테이블이 없는경우 앱 강제 업데이트 요구됨
                //var versionData = orderedEnumerable.FirstOrDefault(x => x.os == ServerSetting.GetOSCode() && x.version == BuildSetting.version);
                var versionData = orderedEnumerable.FirstOrDefault(x => x.os == ServerSetting.GetOSCode() && x.version == 10500);
                ServerSetting.Set(versionData);

                var lastVersionData = orderedEnumerable.FirstOrDefault(x => x.os == ServerSetting.GetOSCode() && x.status == EServerStatus.Live);
                ServerSetting.lastAppVersion = lastVersionData.version;
                Debug.LogFormat("[Server/Setting/LastAppVersion] {0}", ServerSetting.lastAppVersion);
            }
        }
        catch (System.Exception e)
        {
            if (exceptionThrow)
                throw;
        }

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
        Uno = uno;
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

    public static async UniTask LoadFromServer(CancellationToken cancellationToken = default)
    {
        SaveData data = await UnityHttp.Send<SaveData>(RequestContext.Create(ServerCmd.USER_DATA_LOAD,
                new Dictionary<string, List<int>>() { { "date_keys", new List<int>() } },
                defaultRetryHandling: false,
                defaultExceptionHandling: false), cancellationToken);

        data.save_datas.ForEach(x => x.tableName = ConvertToTableName(x.date_key));

        if (useCompress)
        {
            data.save_datas.ForEach(x =>
            {
                if (string.IsNullOrEmpty(x.save_data))
                    return;

                try
                {
                    x.save_data = CLZF2.DecompressFromBase64(x.save_data);
                }
                catch (Exception e) { Debug.LogError(e); }
            });
        }

        foreach (var item in data.save_datas)
        {
            if (item.tableName == "BaseData")
            {
                UserDataManager.Instance.baseData = UserDataManager.Instance.baseData.ConvertToObject<BaseData>(item.save_data);
            }
            else if (item.tableName == "InventoryData")
            {
                UserDataManager.Instance.inventoryData = UserDataManager.Instance.baseData.ConvertToObject<InventoryData>(item.save_data);
            }
        }
        Debug.Log(UserDataManager.Instance.baseData.level);
        Debug.Log($"itemCount : {UserDataManager.Instance.inventoryData.itemList.Count}");
    }

    public static async UniTask SaveToServer(CancellationToken cancellationToken = default)
    {
        SaveData data = new SaveData();
        data.ver = 999;
        data.save_datas = new List<RawData>();
        var tables = tableNames.Values.ToList();
        for (int i = 0; i < tableNames.Count; i++)
        {
            var date_key = ConvertToDateKey(tables[i]);
            var rawData = new RawData();
            rawData.tableName = tables[i];
            rawData.date_key = date_key;

            if (rawData.tableName == "BaseData")
            {
                rawData.save_data = UserDataManager.Instance.baseData.ToJson();
            }
            else if(rawData.tableName == "InventoryData")
            {
                rawData.save_data = UserDataManager.Instance.inventoryData.ToJson();
            }
            
            data.save_datas.Add(rawData);
        }
        await SaveToServer(data, useCompress, cancellationToken: cancellationToken);
        Debug.Log("SaveToServer");

    }
    private static async UniTask SaveToServer(SaveData value, bool compress = false, CancellationToken cancellationToken = default)
    {
        if (compress)
        {
            value.save_datas.ForEach(x =>
            {
                if (string.IsNullOrEmpty(x.save_data))
                    return;

                try
                {
                    x.save_data = CLZF2.CompressToBase64(x.save_data);
                }
                catch (Exception e) { Debug.LogError(e); }
            });
        }
        await UnityHttp.Send(RequestContext.Create(ServerCmd.USER_DATA_SAVE, value,
                    defaultLockHandling: false,
                    defaultRetryHandling: false,
                    defaultExceptionHandling: false), cancellationToken);

#if UNITY_EDITOR
        //[Todo][Classic] 임시 처리
        value.save_datas.ForEach(x => PlayerPrefs.SetString($"{Uno}/{x.tableName}", x.save_data));
#endif
    }
}

