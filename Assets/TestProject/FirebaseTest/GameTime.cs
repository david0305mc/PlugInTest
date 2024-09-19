using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class GameTime 
{
    private static long initGameTime = 0;
    private static float initClientStartupTime = 0;

    public static void InitLocalBase()
    {
        Init((DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds);
    }
    //public static async UniTask InitGameTime()
    //{
    //    string dateTimeString = await Network.NetworkManager.GetNetworkTime();

    //    if (string.IsNullOrEmpty(dateTimeString))
    //    {
    //        InitLocalBase();
    //    }
    //    else
    //    {
    //        Debug.Log($"date {dateTimeString}");
    //        //DateTime localTime = DateTime.Parse(dateTimeString).ToLocalTime();
    //        //DateTime minight = new DateTime(localTime.Year, localTime.Month, localTime.Day + 1);
    //        //Debug.Log($"Korea {localTime}");
    //        //Debug.Log($"Korea midnight {minight}");
    //        //var localTimeSpan = minight.Subtract(localTime);
    //        //Debug.Log($"time left {localTimeSpan}");
    //        Init((DateTime.Parse(dateTimeString) - DateTime.UnixEpoch).TotalSeconds);
    //    }
    //}

    public static void Init(double _gameTime)
    {
        initGameTime = Convert.ToInt64(_gameTime);
        initClientStartupTime = Time.realtimeSinceStartup;
        //Debug.LogFormat("[GameTime/Set] {0}", Get(DateTimeKind.Utc));
    }

    public static long Get()
    {
        if (initGameTime == 0)
        {
            Init((DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds);
        }

        return initGameTime + Convert.ToInt64(Time.realtimeSinceStartup - initClientStartupTime);
    }

    //public static long GetLocalMidnight()
    //{
    //    DateTime localTime = Utill.ConvertFromUnixTimestamp(Get()).ToLocalTime();
    //    Debug.Log($"Korea {localTime}");
    //    DateTime midNightTime = new DateTime(localTime.Year, localTime.Month, localTime.Day + 1);
    //    Debug.Log($"Korea midnight {midNightTime}");
    //    return Convert.ToInt64(Utill.ConvertToUnitxTimeStamp(midNightTime));
    //}

}
