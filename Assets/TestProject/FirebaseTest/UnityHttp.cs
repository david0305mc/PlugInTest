using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Response = Protocols.Response;


public enum RequestType
{
    Get,
    Post
}

public class RequestContext
{
    public string id;
    public int method;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public object @params;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string sess;

    [JsonIgnore] public RequestType reqType;

    /// <summary>재전송 디폴트 처리 여부</summary>
    [JsonIgnore] public bool defaultRetryHandling;

    /// <summary>예외 동작 디폴트 처리 여부</summary>    
    [JsonIgnore] public bool defaultExceptionHandling;

    /// <summary>입력 락 디폴트 처리 여부</summary>    
    [JsonIgnore] public bool defaultLockHandling;


    public static RequestContext Create(int method,
        RequestType requestType = RequestType.Post,
        bool defaultLockHandling = true,
        bool defaultRetryHandling = true,
        bool defaultExceptionHandling = true)
    {
        RequestContext ret = new RequestContext();
        ret.id = Utility.RandomId8Bytes();
        ret.method = method;
        ret.@params = null;
        ret.reqType = requestType;
        ret.sess = ServerSetting.sess;
        ret.defaultLockHandling = defaultLockHandling;
        ret.defaultRetryHandling = defaultRetryHandling;
        ret.defaultExceptionHandling = defaultExceptionHandling;
        return ret;
    }

    public static RequestContext Create<T>(int method,
        T data,
        RequestType requestType = RequestType.Post,
        bool defaultLockHandling = true,
        bool defaultRetryHandling = true,
        bool defaultExceptionHandling = true)
    {
        var ret = Create(method, requestType, defaultLockHandling, defaultRetryHandling, defaultExceptionHandling);
        ret.@params = data;
        return ret;
    }
}

public class ResponseContext
{
    public string id;
    public JToken result;
    public Response.ErrorData error;
    public Response.AlertData alert;
    //public Protocols.Common.MaintenanceData maintenance;
    public double server_time;
    public T GetResult<T>() => result.ToObject<T>();
}

//public class ResponseData
//{
//    readonly byte[] bytes;
//    public Dictionary<string, string> ResponseHeaders { get; }
//    public T GetResult<T>() => JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes));

//    public ResponseData(byte[] bytes, Dictionary<string, string> responseHeaders)
//    {
//        this.bytes = bytes;
//        this.ResponseHeaders = responseHeaders;
//    }
//}

public static class UnityHttp
{
    private const string systemMessage = "system";

    private static Dictionary<string, string> jsonHeaders = new Dictionary<string, string>() { { "Content-Type", "application/json" } };
    private static readonly int timeout = 30;
    private static readonly int retryTimeout = 8;
    private static bool retryChecking = false;
    private static double retryLastTime;
    //private static double retryElapsedTime => GameTime.Get() - retryLastTime;
    //private static bool isRetryTimeOut => retryElapsedTime > retryTimeout;
    public static double server_time { get; private set; }


    //    //ResponseHeader 정보가 필요한 경우에 사용        
    //    public static async UniTask<ResponseData> GetData(string url,
    //        Dictionary<string, string> headers = null,
    //        IProgress<float> progress = null,
    //        CancellationToken cancellationToken = default)
    //    {
    //        RETRY:
    //        using (UnityWebRequest req = UnityWebRequest.Get(Utility.URLAntiCacheRandomizer(url)))
    //        {
    //            req.timeout = timeout;

    //            if (headers != null)
    //            {
    //                foreach (var header in headers)
    //                    req.SetRequestHeader(header.Key, header.Value);
    //            }

    //            try
    //            {
    //                await req.SendWebRequest().ToUniTask(progress, cancellationToken: cancellationToken);
    //#if ENABLE_HTTP_LOG
    //                Debug.LogFormat("[UnityHttp/Get/Recv] {0}", req.downloadHandler.text);
    //#endif
    //                return new ResponseData(req.downloadHandler.data, req.GetResponseHeaders());
    //            }
    //            catch (UnityWebRequestException e)
    //            {
    //                Debug.LogErrorFormat("[UnityHttp] {0}", e);
    //                await CheckRetryTimeout(cancellationToken);
    //                goto RETRY;
    //            }
    //        }
    //    }

    public static async UniTask<byte[]> Get(string url,
    Dictionary<string, string> headers = null,
    IProgress<float> progress = null,
    bool defaultLockHandling = true,
    bool defaultRetryHandling = true,
    bool defaultExceptionHandling = true,
    CancellationToken cancellationToken = default)
{
    RETRY:
    using (UnityWebRequest req = UnityWebRequest.Get(Utility.URLAntiCacheRandomizer(url)))
    {
        req.timeout = timeout;

        if (headers != null)
        {
            foreach (var header in headers)
                req.SetRequestHeader(header.Key, header.Value);
        }

        try
        {
            await req.SendWebRequest().ToUniTask(progress, cancellationToken: cancellationToken);
#if ENABLE_HTTP_LOG
                Debug.LogFormat("[UnityHttp/Get/Recv] {0}", req.downloadHandler.text);
#endif
            return req.downloadHandler.data;
        }
        catch (UnityWebRequestException e)
        {
            Debug.LogErrorFormat("[UnityHttp] {0}\n{1}", url, e);

            if (!defaultExceptionHandling)
                throw;

            if (!defaultRetryHandling)
                throw;

            //await CheckRetryTimeout(cancellationToken);
            goto RETRY;
        }
    }
}

    public static async UniTask<T> Get<T>(string url,
        Dictionary<string, string> headers = null,
        IProgress<float> progress = null,
        bool defaultLockHandling = true,
        bool defaultRetryHandling = true,
        bool defaultExceptionHandling = true,
        CancellationToken cancellationToken = default)
    {
        return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(await Get(url, headers, progress, defaultLockHandling, defaultRetryHandling, defaultExceptionHandling, cancellationToken)));
    }
    /// <summary>
    /// 서버 요청 처리
    /// <para/>(주의!)정상적인 응답이 아닌경우 Exception 발생시키므로 응답 대기 로직의 경우 반드시 예외처리 필요!!    
    /// </summary>    
    /// 외부에서 재시작 처리하게 된 경우 중단처리 되어야한다
    /// [Todo] 성능 개선을 위해 JsonSerialize 분리
    public static async UniTask<ResponseContext> Send(RequestContext data, CancellationToken cancellationToken = default)
    {
        if (data.defaultLockHandling)
        {
            TouchBlockManager.Instance.AddLock();
        }

        try
        {
            string id = data.id;
            string jsonData = JsonConvert.SerializeObject(data);
            //string id = "2bfbf719";
            //string jsonData = "{\"id\":\"2bfbf719\",\"method\":1000,\"params\":{\"platform\":3,\"platform_id\":\"eyJhbGciOiJSUzI1NiIsImtpZCI6ImUwM2E2ODg3YWU3ZjNkMTAyNzNjNjRiMDU3ZTY1MzE1MWUyOTBiNzIiLCJ0eXAiOiJKV1QifQ.eyJwcm92aWRlcl9pZCI6ImFub255bW91cyIsImlzcyI6Imh0dHBzOi8vc2VjdXJldG9rZW4uZ29vZ2xlLmNvbS9hYnlzc2NsYXNzaWMiLCJhdWQiOiJhYnlzc2NsYXNzaWMiLCJhdXRoX3RpbWUiOjE3MjY3Mzk0NjgsInVzZXJfaWQiOiJGV213T2MxdTZDWE9mR05Pb1ZlMWlqaGRLVEEzIiwic3ViIjoiRldtd09jMXU2Q1hPZkdOT29WZTFpamhkS1RBMyIsImlhdCI6MTcyNjczOTQ2OSwiZXhwIjoxNzI2NzQzMDY5LCJmaXJlYmFzZSI6eyJpZGVudGl0aWVzIjp7fSwic2lnbl9pbl9wcm92aWRlciI6ImFub255bW91cyJ9fQ.TyIBMuG6tXtismvH2DmIgg7_7pP9FDzFRFJUaIF5JGymVrnJdD_1_GtwxBVa5gWOpZOrh8Eg7vjEqWuPsmUlBqxOQnS1z1V_zDT5tJXWIqXroK5Pd4esrlWSBXdKg2QmLeIbh5hnK-29bi_cVvb7lwzWmkRjvF8kbmi-o2yePmfABI2XV3STBLf-hdQT-8A8-syvtJrtiX0fygBfHsfFGHZIqf-aT9iOKj5cvCbPo47hLqWoSTwwjBMxZ8nuLLU-L0sSqW-rAFU0xERxqrMY7pF8FOdV1njZbI1RS-GrCqJRwoZjI_nSvdRACVvD3-4DXSgi9GiY0v5Ve50T9WcGCg\",\"lang\":\"KO\",\"push_id\":\"\",\"os\":1,\"gpresto_sdata\":null,\"gpresto_engine_state\":0}}";
            Encryptor encryptor = ServerSetting.isEncryptServer ? ServerSetting.encryptor : null;
            byte[] rawData = encryptor != null ? Encoding.UTF8.GetBytes(await encryptor.EncryptToStringAsync(jsonData)) : Encoding.UTF8.GetBytes(jsonData);

#if ENABLE_HTTP_LOG
            Debug.LogFormat("[UnityHttp/Post/Send]{0} Bytes\n{1}", Utility.FormatBytes(rawData.Length), jsonData);
#endif

            RETRY:
            using (UnityWebRequest req = new UnityWebRequest(ServerSetting.gameUrl, UnityWebRequest.kHttpVerbPOST))
            {
                req.timeout = timeout;

                foreach (var header in jsonHeaders)
                    req.SetRequestHeader(header.Key, header.Value);

                req.downloadHandler = new DownloadHandlerBuffer();
                req.uploadHandler = new UploadHandlerRaw(rawData);
                string text = string.Empty;

                try
                {
                    await req.SendWebRequest().ToUniTask(cancellationToken: cancellationToken);

                    text = encryptor != null ? await encryptor.DecryptToStringAsync(req.downloadHandler.text) : req.downloadHandler.text;
#if ENABLE_HTTP_LOG
                    Debug.LogFormat("[UnityHttp/Post/Recv] {0}", text);
#endif                    
                }
                catch (UnityWebRequestException e)
                {
                    Debug.LogErrorFormat("[UnityHttp] {0}:{1}", id, e);

                    if (data.reqType == RequestType.Post && e.Result != UnityWebRequest.Result.ConnectionError)
                    {
                        var exception = new UnityHttpNetworkException(req);
                        if (data.defaultExceptionHandling)
                            await ServerAPI.Exception(exception, cancellationToken);

                        throw exception;
                    }

                    if (!data.defaultRetryHandling)
                    {
                        var exception = new UnityHttpNetworkException(req);
                        if (data.defaultExceptionHandling)
                            await ServerAPI.Exception(exception, cancellationToken);

                        throw exception;
                    }

                    await CheckRetryTimeout(cancellationToken);
                    goto RETRY;
                }


                ResponseContext resp = null;
                try
                {
                    resp = JsonConvert.DeserializeObject<ResponseContext>(text);
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("[UnityHttp] {0}:{1}", id, e);

                    var resps = JsonConvert.DeserializeObject<ResponseContext[]>(text);
                    for (int i = 0; i < resps.Length; ++i)
                    {
                        if (resps[i].id == systemMessage)
                        {
                            Debug.LogError("[UnityHttp/Send/Recv] SystemMessage");
                            //await GameAPI.OnSystemMessage(resps[i].GetResult<Response.SystemData>(), cancellationToken);
                        }
                        else if (resps[i].id == id)
                        {
                            resp = resps[i];
                        }
                    }
                }

                if (resp == null)
                    throw new UnityHttpNetworkException(req);

                if (resp.error != null)
                {
                    //응답 error 기본 동작 : 앱 종료                                        
                    Debug.LogErrorFormat("[UnityHttp/Send/Recv] {0} Error code : {1}", id, resp.error.code);

//                    if (resp.error.code == ServerErrorCode.AUTH_SESSION_NOT_MATCHING)
//                    {
//                        //세션 변경된 경우 강제 인트로 이동 처리
//                        //await GameAPI.ShowMessage(resp.error, cancellationToken);

//                        Application.Quit();
//#if UNITY_EDITOR
//                        UnityEditor.EditorApplication.isPlaying = false;
//#endif
//                        throw new System.OperationCanceledException();
//                    }

//                    var exception = new UnityHttpGameServerException(resp);
//                    if (data.defaultExceptionHandling)
//                        await GameAPI.Exception(exception, cancellationToken);

//                    throw exception;
                }

                if (resp.alert != null)
                {
                    //응답 alert 기본 동작 : 알림 메시지 처리

                    Debug.LogErrorFormat("[UnityHttp/Send/Recv] {0} Alert code : {1}", id, resp.alert.code);

//                    if (resp.alert.code == ServerErrorCode.AUTH_SESSION_NOT_MATCHING)
//                    {
//                        //세션 변경된 경우 강제 인트로 이동 처리
//                        await GameAPI.ShowMessage(resp.alert, cancellationToken);

//                        Application.Quit();
//#if UNITY_EDITOR
//                        UnityEditor.EditorApplication.isPlaying = false;
//#endif
//                        throw new System.OperationCanceledException();
//                    }

//                    var exception = new UnityHttpGameServerException(resp);
//                    if (data.defaultExceptionHandling)
//                        await GameAPI.Exception(exception, cancellationToken);

//                    throw exception;
                }

                //if (resp.maintenance != null)
                //{
                //    //점검 기본 동작 : 인트로 이동

                //    Debug.LogError("[UnityHttp/Send/Recv] Maintenance");

                //    var exception = new UnityHttpGameServerMaintenance(resp.maintenance);
                //    if (data.defaultExceptionHandling)
                //        await GameAPI.Exception(exception, cancellationToken);

                //    MessageDispather.Publish(exception);
                //    throw exception;
                //}

                server_time = resp.server_time;
                return resp;
            }
        }
        finally
        {
            if (data.defaultLockHandling)
            {
                TouchBlockManager.Instance.RemoveLock();
            }
        }
    }

    static async UniTask CheckRetryTimeout(CancellationToken cancellationToken)
    {
        Debug.LogError("CheckRetryTimeout");
        await UniTask.WaitForSeconds(1000f);
        //await UniTask.WaitWhile(() => retryChecking, cancellationToken: cancellationToken);

        //if (!isRetryTimeOut)
        //{
        //    await UniTask.Delay(TimeSpan.FromSeconds(2.0), cancellationToken: cancellationToken);
        //    return;
        //}

        //try
        //{
        //    retryChecking = true;
        //    await MessageDispather.PublishAsync(EMessage.Confirm, Localization.Get("system_error_check_network"), cancellationToken);
        //}
        //finally
        //{
        //    retryChecking = false;
        //    retryLastTime = GameTime.Get();
        //}
    }
    public static UniTask<T> Send<T>(RequestContext data,
        CancellationToken cancellationToken = default)
    {
        return Send(data, cancellationToken).ContinueWith(x => x.GetResult<T>());
    }

    //    static async UniTask CheckRetryTimeout(CancellationToken cancellationToken)
    //    {
    //        await UniTask.WaitWhile(() => retryChecking, cancellationToken: cancellationToken);

    //        if (!isRetryTimeOut)
    //        {
    //            await UniTask.Delay(TimeSpan.FromSeconds(2.0), cancellationToken: cancellationToken);
    //            return;
    //        }

    //        try
    //        {
    //            retryChecking = true;
    //            await MessageDispather.PublishAsync(EMessage.Confirm, Localization.Get("system_error_check_network"), cancellationToken);
    //        }
    //        finally
    //        {
    //            retryChecking = false;
    //            retryLastTime = GameTime.Get();
    //        }
    //    }
}




public class UnityHttpException : Exception
{

}


/// <summary>네트워크 에러</summary>
public class UnityHttpNetworkException : UnityHttpException
{
    public UnityWebRequest UnityWebRequest { get; }
#if UNITY_2020_2_OR_NEWER
    public UnityWebRequest.Result Result { get; }
#else
        public bool IsNetworkError { get; }
        public bool IsHttpError { get; }
#endif
    public string Error { get; }
    public string Text { get; }
    public long ResponseCode { get; }
    public Dictionary<string, string> ResponseHeaders { get; }

    string msg;

    public UnityHttpNetworkException() { }

    public UnityHttpNetworkException(UnityWebRequest unityWebRequest)
    {
        this.UnityWebRequest = unityWebRequest;
#if UNITY_2020_2_OR_NEWER
        this.Result = unityWebRequest.result;
#else
        this.IsNetworkError = unityWebRequest.isNetworkError;
        this.IsHttpError = unityWebRequest.isHttpError;
#endif
        this.Error = unityWebRequest.error;
        this.ResponseCode = unityWebRequest.responseCode;
        if (UnityWebRequest.downloadHandler != null)
        {
            if (unityWebRequest.downloadHandler is DownloadHandlerBuffer dhb)
            {
                this.Text = dhb.text;
            }
        }
        this.ResponseHeaders = unityWebRequest.GetResponseHeaders();
    }

    public override string Message
    {
        get
        {
            if (msg == null)
            {
                if (Text != null)
                {
                    msg = Error + Environment.NewLine + Text;
                }
                else
                {
                    msg = Error;
                }
            }
            return msg;
        }
    }
}

///// <summary>게임 서버 에러</summary>
//public class UnityHttpGameServerException : UnityHttpException
//{
//    public ResponseContext ResponseData { get; }
//    public int Code
//    {
//        get
//        {
//            if (ResponseData.alert != null)
//                return ResponseData.alert.code;

//            if (ResponseData.error != null)
//                return ResponseData.error.code;

//            return 0;
//        }
//    }

//    public UnityHttpGameServerException(ResponseContext data) => ResponseData = data;
//}

///// <summary>게임 서버 점검</summary>
//public class UnityHttpGameServerMaintenance : UnityHttpException
//{
//    //public Protocols.Common.MaintenanceData ResponseData { get; }
//    //public UnityHttpGameServerMaintenance(Protocols.Common.MaintenanceData data) => ResponseData = data;
//}

///// <summary>내부 로직 에러</summary>
//public class UnityIntenalException : Exception
//{
//    public int code;

//    public UnityIntenalException(int code)
//    {
//        this.code = code;
//    }
//}

///// <summary>지급된 상품</summary>
//public class BillingAlreadyPaidException : UnityHttpGameServerException
//{
//    public BillingAlreadyPaidException(ResponseContext data) : base(data) { }
//}

///// <summary>탈퇴 대기중인 유저</summary>
//public class LeaveUserException : Exception
//{
//    public ulong uno;
//    public string token;
//    public double leave_time;

//    public LeaveUserException(Response.SignIn data)
//    {
//        uno = data.uno;
//        token = data.token;
//        leave_time = data.leave_time;
//    }
//}
