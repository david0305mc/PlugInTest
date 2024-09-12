using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility
{
    public static string URLAntiCacheRandomizer(string url)
    {
        string r = "";
        r += UnityEngine.Random.Range(1000000, 8000000).ToString();
        r += UnityEngine.Random.Range(1000000, 8000000).ToString();
        return url + "?p=" + r;
    }
    public static string RandomId8Bytes()
    {
        byte[] arr = new byte[4];
        for (int idx = 0; idx < arr.Length; ++idx)
        {
            int i = UnityEngine.Random.Range(0, 256);
            arr[idx] = (byte)i;
        }
        return string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", arr[0], arr[1], arr[2], arr[3]).ToLower();
    }

}
