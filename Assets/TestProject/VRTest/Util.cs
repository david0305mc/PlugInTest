using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public static class Util 
{
    public static void Clear(this CancellationTokenSource cts)
    {
        if (cts.IsCancellationRequested)
            return;
        cts.Cancel();
        cts.Dispose();
    }

}
