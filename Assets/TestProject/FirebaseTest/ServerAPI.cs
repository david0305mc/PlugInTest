using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Linq;

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

    public static async UniTask SignIn(EPlatform platform, string platformId, string lang, string push_id, CancellationToken cancellationToken = default)
    { 
        
    }
}
