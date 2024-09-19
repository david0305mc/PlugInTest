using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class TouchBlockManager : Singleton<TouchBlockManager>
{
    private readonly int ProgressWaitTime = 100;

    private List<PanelTouchBlock> lockPanelLists = new List<PanelTouchBlock>();
    private int uiLockCount;
    private CancellationTokenSource tokenSource;

    public void AddLock()
    {
        uiLockCount++;
        if (uiLockCount == 1)
        {
            UpdateLock();
        }
    }

    public void RemoveLock()
    {
        uiLockCount--;
        if (uiLockCount == 0)
        {
            UpdateLock();
        }
        Debug.Assert(uiLockCount >= 0, "it must not be less than 0");
    }

    public bool IsLock() => uiLockCount > 0;

    private void UpdateLock()
    {
        if (uiLockCount > 0)
        {
            if (uiLockCount == 1)
            {
                tokenSource?.Clear();
                tokenSource = new CancellationTokenSource();
                SetTouchBlock(true);

                UniTask.Create(async () =>
                {
                    await UniTask.Delay(ProgressWaitTime, cancellationToken: tokenSource.Token);
                    SetProgressObject(true);
                });
            }
        }
        else
        {
            tokenSource?.Clear();
            tokenSource = null;

            SetTouchBlock(false);
            SetProgressObject(false);
        }
    }

    private void SetTouchBlock(bool val)
    {
        foreach (var panel in lockPanelLists)
        {
            panel.SetTouchBlock(val);
        }
    }

    private void SetProgressObject(bool val)
    {
        foreach (var panel in lockPanelLists)
        {
            panel.SetProgressObject(val);
        }
    }

    public void RegisterLockPanel(PanelTouchBlock panel)
    {
        lockPanelLists.Add(panel);
    }

    public void UnRegisterLockPanel(PanelTouchBlock panel)
    {
        lockPanelLists.Remove(panel);
    }
}
