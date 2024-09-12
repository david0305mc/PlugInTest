using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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