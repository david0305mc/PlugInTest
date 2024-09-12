using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EServerStatus
{
    /// <summary>라이브</summary>
    Live,
    /// <summary>리뷰</summary>
    Review = 5,
    /// <summary>업데이트 권장</summary>
    Update_Recommend,
    /// <summary>업데이트 강제</summary>
    Update_Essential,
    /// <summary>점검</summary>
    Maintenance,
}
public class VersionData
{
    public int version;
    public OSCode os;
    public EServerStatus status;
    public string game_url;
    public string cdn_url;
    public int coupon_use;
}

public enum OSCode
{
    Unknown = -1,
    iOS = 0,
    Android = 1,
    Windows = Android,
}
public enum EServerType
{
    Dev,
    Qa,
    Review,
    Live,
}
public enum EPlatform
{
    None,
    Google,
    Apple,
    Guest,
    Webus,
    DevWindows = 90,
    Unknown,
    Deleted,
}

public enum EBuildType
{
    Dev,
    Release,
}

public static class BuildSetting
{
    public static readonly EBuildType type = EBuildType.Dev;
    public static int version;

    static BuildSetting()
    {
#if DEV
        type = EBuildType.Dev;
#else
        type = EBuildType.Release;
#endif

        var versionSplit = Application.version.Split('.');
        int major = int.Parse(versionSplit[0]) * 10000;
        int minor = int.Parse(versionSplit[1]) * 100;
        int build = versionSplit.Length > 2 ? int.Parse(versionSplit[2]) : 0;
        version = major + minor + build;
    }
}